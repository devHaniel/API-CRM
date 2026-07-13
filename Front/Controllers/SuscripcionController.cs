using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Configuration;
using Microsoft.Extensions.Options;

namespace Front.Controllers
{
    [Authorize]
    public class SuscripcionController : Controller
    {
        private readonly IPlanService _planService;
        private readonly IStripeService _stripeService;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly StripeSettings _stripeSettings;

        public SuscripcionController(
            IPlanService planService,
            IStripeService stripeService,
            ICurrentTenantService currentTenantService,
            IOptions<StripeSettings> stripeSettings)
        {
            _planService = planService;
            _stripeService = stripeService;
            _currentTenantService = currentTenantService;
            _stripeSettings = stripeSettings.Value;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;
            var planes = await _planService.ObtenerTodosAsync(1, 100, ct);
            var planActual = await ObtenerPlanActualAsync(tenantId, ct);

            ViewData["PlanActualId"] = planActual?.Id;
            ViewData["PlanMaxPrecio"] = planes.Items.Max(p => p.PrecioMensual);
            ViewData["StripePublishableKey"] = _stripeSettings.PublishableKey;

            return View(planes.Items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Guid planId, CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;

            try
            {
                if (await TienePlanMaximoAsync(tenantId, ct))
                {
                    TempData["Error"] = "Ya estás en el plan más alto disponible.";
                    return RedirectToAction(nameof(Index));
                }

                await _planService.CambiarPlanDelTenantAsync(tenantId, planId, ct);

                var successUrl = Url.Action(nameof(Exito), "Suscripcion", null, Request.Scheme)
                    ?? throw new InvalidOperationException("No se pudo generar la URL de éxito.");

                var cancelUrl = Url.Action(nameof(Cancelado), "Suscripcion", null, Request.Scheme)
                    ?? throw new InvalidOperationException("No se pudo generar la URL de cancelación.");

                var checkoutUrl = await _stripeService.CrearCheckoutSessionAsync(
                    tenantId, planId, successUrl, cancelUrl, ct);

                return Redirect(checkoutUrl);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [AllowAnonymous]
        public IActionResult Exito()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Cancelado()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IrAlPortal(CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;
            var returnUrl = Url.Action(nameof(Index), "Suscripcion", null, Request.Scheme)
                ?? throw new InvalidOperationException("No se pudo generar la URL de retorno.");

            try
            {
                var portalUrl = await _stripeService.GenerarPortalUrlAsync(tenantId, returnUrl, ct);
                return Redirect(portalUrl);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarSuscripcion(CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;

            try
            {
                await _stripeService.CancelarSuscripcionAsync(tenantId, ct);
                TempData["Mensaje"] = "Tu suscripción se cancelará al final del periodo actual.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<Application.DTOs.Plan.PlanDto?> ObtenerPlanActualAsync(Guid tenantId, CancellationToken ct)
        {
            try
            {
                var plan = await _planService.ObtenerPlanDelTenantAsync(tenantId, ct);
                return new Application.DTOs.Plan.PlanDto(
                    plan.PlanId, plan.Nombre, plan.PrecioMensual, plan.LimiteRecordatoriosMes,
                    plan.PrecioRecordatorioExtra, plan.MaxUsuarios, true, default);
            }
            catch
            {
                return null;
            }
        }

        private async Task<bool> TienePlanMaximoAsync(Guid tenantId, CancellationToken ct)
        {
            var planes = await _planService.ObtenerTodosAsync(1, 100, ct);
            var actual = await ObtenerPlanActualAsync(tenantId, ct);
            return actual is not null && actual.PrecioMensual >= planes.Items.Max(p => p.PrecioMensual);
        }
    }
}
