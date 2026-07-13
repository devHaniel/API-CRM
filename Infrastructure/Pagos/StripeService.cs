using Application.Configuration;
using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Infrastructure.Pagos
{
    public class StripeService : IStripeService
    {
        private readonly StripeSettings _settings;
        private readonly ITenantRepository _tenantRepository;
        private readonly IPlanRepository _planRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<StripeService> _logger;

        public StripeService(
            IOptions<StripeSettings> settings,
            ITenantRepository tenantRepository,
            IPlanRepository planRepository,
            IUsuarioRepository usuarioRepository,
            IEmailService emailService,
            ILogger<StripeService> logger)
        {
            _settings = settings.Value;
            _tenantRepository = tenantRepository;
            _planRepository = planRepository;
            _usuarioRepository = usuarioRepository;
            _emailService = emailService;
            _logger = logger;
            StripeConfiguration.ApiKey = _settings.SecretKey;
        }

        public async Task<string> CrearCheckoutSessionAsync(
            Guid tenantId, Guid planId, string successUrl, string cancelUrl, CancellationToken ct = default)
        {
            var plan = await _planRepository.GetByIdAsync(planId, ct)
                ?? throw new KeyNotFoundException("Plan no encontrado.");

            if (!plan.Activo)
                throw new InvalidOperationException("El plan no está disponible.");

            if (string.IsNullOrWhiteSpace(plan.StripePriceId))
                throw new InvalidOperationException("El plan no tiene un precio de Stripe configurado.");

            var customerId = await ObtenerOCrearClienteStripeAsync(tenantId, ct);

            var options = new SessionCreateOptions
            {
                Customer = customerId,
                Mode = "subscription",
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price = plan.StripePriceId,
                        Quantity = 1,
                    }
                ],
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "tenant_id", tenantId.ToString() },
                    { "plan_id", planId.ToString() }
                },
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options, cancellationToken: ct);

            _logger.LogInformation(
                "Checkout session {SessionId} creada para tenant {TenantId}, plan {PlanId}",
                session.Id, tenantId, planId);

            return session.Url;
        }

        public async Task<string?> ObtenerStripeCustomerIdAsync(Guid tenantId, CancellationToken ct = default)
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, ct);
            return tenant?.StripeCustomerId;
        }

        public async Task<Guid?> ConfirmarCheckoutSessionAsync(string sessionId, CancellationToken ct = default)
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId, cancellationToken: ct);

            if (session.Status != "complete")
                return null;

            if (!session.Metadata.TryGetValue("plan_id", out var planIdStr) ||
                !Guid.TryParse(planIdStr, out var planId))
                return null;

            if (!session.Metadata.TryGetValue("tenant_id", out var tenantIdStr) ||
                !Guid.TryParse(tenantIdStr, out var tenantId))
                return null;

            try
            {
                await _planRepository.AsignarPlanAlTenantAsync(tenantId, planId, ct);
                await _planRepository.SaveChangesAsync(ct);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Tenant {TenantId} no encontrado al confirmar sesión {SessionId}", tenantId, sessionId);
                return null;
            }

            _logger.LogInformation(
                "Checkout session {SessionId} confirmada: tenant {TenantId} asignado al plan {PlanId}",
                sessionId, tenantId, planId);

            return planId;
        }

        public async Task CancelarSuscripcionAsync(Guid tenantId, CancellationToken ct = default)
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, ct)
                ?? throw new KeyNotFoundException("Tenant no encontrado.");

            if (string.IsNullOrWhiteSpace(tenant.StripeCustomerId))
                throw new InvalidOperationException("El tenant no tiene un cliente de Stripe asociado.");

            var subscriptionService = new SubscriptionService();
            var subscriptions = await subscriptionService.ListAsync(
                new SubscriptionListOptions
                {
                    Customer = tenant.StripeCustomerId,
                    Status = "active",
                }, cancellationToken: ct);

            var subscription = subscriptions.FirstOrDefault()
                ?? throw new InvalidOperationException("No hay una suscripción activa para cancelar.");

            var options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true,
            };

            await subscriptionService.UpdateAsync(subscription.Id, options, cancellationToken: ct);

            _logger.LogInformation(
                "Suscripción {SubscriptionId} programada para cancelar al final del periodo para tenant {TenantId}",
                subscription.Id, tenantId);
        }

        public async Task<string> GenerarPortalUrlAsync(Guid tenantId, string returnUrl, CancellationToken ct = default)
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, ct)
                ?? throw new KeyNotFoundException("Tenant no encontrado.");

            if (string.IsNullOrWhiteSpace(tenant.StripeCustomerId))
                throw new InvalidOperationException("El tenant no tiene un cliente de Stripe asociado.");

            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = tenant.StripeCustomerId,
                ReturnUrl = returnUrl,
            };

            var service = new Stripe.BillingPortal.SessionService();
            var session = await service.CreateAsync(options, cancellationToken: ct);

            _logger.LogInformation(
                "Portal session {SessionId} creada para tenant {TenantId}",
                session.Id, tenantId);

            return session.Url;
        }

        public async Task<bool> ProcesarEventoWebhookAsync(string json, string stripeSignature, CancellationToken ct = default)
        {
            try
            {
                var evento = EventUtility.ConstructEvent(json, stripeSignature, _settings.WebhookSecret);

                switch (evento.Type)
                {
                    case "checkout.session.completed":
                        await ProcesarCheckoutCompletadoAsync(evento, ct);
                        return true;

                    case "invoice.paid":
                        await ProcesarFacturaPagadaAsync(evento, ct);
                        return true;

                    case "customer.subscription.deleted":
                        await ProcesarSuscripcionCanceladaAsync(evento, ct);
                        return true;

                    case "invoice.payment_failed":
                        await ProcesarPagoFallidoAsync(evento, ct);
                        return true;

                    default:
                        _logger.LogInformation("Evento Stripe no manejado: {Tipo}", evento.Type);
                        return false;
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error al procesar webhook de Stripe");
                throw;
            }
        }

        private async Task<string> ObtenerOCrearClienteStripeAsync(Guid tenantId, CancellationToken ct = default)
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, ct)
                ?? throw new KeyNotFoundException("Tenant no encontrado.");

            if (!string.IsNullOrWhiteSpace(tenant.StripeCustomerId))
                return tenant.StripeCustomerId;

            var options = new CustomerCreateOptions
            {
                Name = tenant.Nombre,
                Metadata = new Dictionary<string, string>
                {
                    { "tenant_id", tenantId.ToString() }
                },
            };

            var service = new CustomerService();
            var customer = await service.CreateAsync(options, cancellationToken: ct);

            tenant.StripeCustomerId = customer.Id;
            _tenantRepository.Update(tenant);
            await _tenantRepository.SaveChangesAsync(ct);

            _logger.LogInformation("Cliente Stripe {CustomerId} creado para tenant {TenantId}", customer.Id, tenantId);

            return customer.Id;
        }

        private async Task ProcesarCheckoutCompletadoAsync(Event evento, CancellationToken ct = default)
        {
            var session = evento.Data.Object as Session;
            if (session is null) return;

            if (!session.Metadata.TryGetValue("tenant_id", out var tenantIdStr) ||
                !Guid.TryParse(tenantIdStr, out var tenantId))
            {
                _logger.LogWarning("Checkout session {SessionId} sin tenant_id en metadata", session.Id);
                return;
            }

            if (!session.Metadata.TryGetValue("plan_id", out var planIdStr) ||
                !Guid.TryParse(planIdStr, out var planId))
            {
                _logger.LogWarning("Checkout session {SessionId} sin plan_id en metadata", session.Id);
                return;
            }

            await _planRepository.AsignarPlanAlTenantAsync(tenantId, planId, ct);
            await _planRepository.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Checkout completado: tenant {TenantId} asignado al plan {PlanId}",
                tenantId, planId);
        }

        private async Task ProcesarFacturaPagadaAsync(Event evento, CancellationToken ct = default)
        {
            var invoice = evento.Data.Object as Invoice;
            if (invoice is null) return;

            var subscriptionId = invoice.SubscriptionId;
            if (string.IsNullOrWhiteSpace(subscriptionId)) return;

            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.GetAsync(subscriptionId, cancellationToken: ct);

            if (!subscription.Metadata.TryGetValue("tenant_id", out var tenantIdStr) ||
                !Guid.TryParse(tenantIdStr, out var tenantId))
            {
                _logger.LogWarning("Suscripción {SubscriptionId} sin tenant_id en metadata", subscriptionId);
                return;
            }

            _logger.LogInformation(
                "Factura pagada para tenant {TenantId}, suscripción {SubscriptionId}",
                tenantId, subscriptionId);
        }

        private async Task ProcesarSuscripcionCanceladaAsync(Event evento, CancellationToken ct = default)
        {
            var subscription = evento.Data.Object as Subscription;
            if (subscription is null) return;

            if (!subscription.Metadata.TryGetValue("tenant_id", out var tenantIdStr) ||
                !Guid.TryParse(tenantIdStr, out var tenantId))
            {
                _logger.LogWarning("Suscripción cancelada {SubscriptionId} sin tenant_id en metadata", subscription.Id);
                return;
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId, ct);
            if (tenant is null) return;

            tenant.PlanId = null;
            _tenantRepository.Update(tenant);
            await _tenantRepository.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Suscripción cancelada: tenant {TenantId}, se eliminó su plan", tenantId);
        }

        private async Task ProcesarPagoFallidoAsync(Event evento, CancellationToken ct = default)
        {
            var invoice = evento.Data.Object as Invoice;
            if (invoice is null) return;

            var subscriptionId = invoice.SubscriptionId;
            if (string.IsNullOrWhiteSpace(subscriptionId)) return;

            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.GetAsync(subscriptionId, cancellationToken: ct);

            if (!subscription.Metadata.TryGetValue("tenant_id", out var tenantIdStr) ||
                !Guid.TryParse(tenantIdStr, out var tenantId))
            {
                _logger.LogWarning("Suscripción {SubscriptionId} sin tenant_id en metadata", subscriptionId);
                return;
            }

            var admin = await _usuarioRepository.GetAdminByTenantAsync(tenantId, ct);
            if (admin is null)
            {
                _logger.LogWarning("No se encontró admin para tenant {TenantId} al notificar pago fallido", tenantId);
                return;
            }

            var daysRemaining = invoice.NextPaymentAttempt.HasValue
                ? (int)(invoice.NextPaymentAttempt.Value.AddDays(3) - DateTime.UtcNow).TotalDays
                : 3;
            var subject = "⚠️ Pago de suscripción fallido - RecorApp";
            var body = $"""
                <h2>Hola {admin.Email},</h2>
                <p>El pago de tu suscripción en <strong>RecorApp</strong> ha fallado.</p>
                <p><strong>Factura:</strong> {invoice.Number}</p>
                <p><strong>Monto:</strong> {(invoice.AmountDue / 100m):C}</p>
                <p><strong>Último intento:</strong> {invoice.NextPaymentAttempt?.ToString("dd/MM/yyyy") ?? "N/A"}</p>
                <p>Stripe reintentará el cobro automáticamente. Si el pago no se completa, tu cuenta podría ser suspendida.</p>
                <p>Actualiza tu método de pago en tu panel de suscripción para evitar interrupciones.</p>
                <hr>
                <p style="color: #6b7280; font-size: 0.875rem;">RecorApp - CRM para tu negocio</p>
                """;

            await _emailService.SendEmailAsync(admin.Email, subject, body, ct);

            _logger.LogInformation(
                "Pago fallido notificado a {Email} para tenant {TenantId}, factura {InvoiceNumber}",
                admin.Email, tenantId, invoice.Number);
        }
    }
}
