namespace Application.Interfaces
{
    public interface IStripeService
    {
    Task<string> CrearCheckoutSessionAsync(Guid tenantId, Guid planId, string successUrl, string cancelUrl, CancellationToken ct = default);
    Task<string?> ObtenerStripeCustomerIdAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> ProcesarEventoWebhookAsync(string json, string stripeSignature, CancellationToken ct = default);
    Task<Guid?> ConfirmarCheckoutSessionAsync(string sessionId, CancellationToken ct = default);
    Task CancelarSuscripcionAsync(Guid tenantId, CancellationToken ct = default);
    Task<string> GenerarPortalUrlAsync(Guid tenantId, string returnUrl, CancellationToken ct = default);
    }
}
