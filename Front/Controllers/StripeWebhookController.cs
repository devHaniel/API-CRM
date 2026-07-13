using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Front.Controllers
{
    [Route("api/stripe/webhook")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IStripeService _stripeService;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(IStripeService stripeService, ILogger<StripeWebhookController> logger)
        {
            _stripeService = stripeService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook(CancellationToken ct)
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(ct);
            var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault()
                ?? throw new InvalidOperationException("Firma de Stripe no encontrada.");

            try
            {
                var procesado = await _stripeService.ProcesarEventoWebhookAsync(json, stripeSignature, ct);
                if (procesado)
                    _logger.LogInformation("Webhook procesado exitosamente");
                else
                    _logger.LogInformation("Webhook recibido pero no procesado (evento no manejado)");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando webhook de Stripe");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
