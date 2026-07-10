using Application.Interfaces;
using Domain.Interfaces;
using Front.Models.Pagos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Front.Controllers
{
    [Authorize]
    public class PagosController : Controller
    {
        private readonly IEventoService _eventoService;
        private readonly ICurrentTenantService _currentTenantService;

        public PagosController(IEventoService eventoService, ICurrentTenantService currentTenantService)
        {
            _eventoService = eventoService;
            _currentTenantService = currentTenantService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;

            var pendientes = await _eventoService.ObtenerPendientesDePagoAsync(tenantId, ct);

            var model = pendientes
                .OrderBy(e => e.Fecha)
                .Select(e => new PagoListItemViewModel
                {
                    Id = e.Id,
                    ClienteNombre = e.ClienteNombre,
                    Descripcion = e.Descripcion,
                    Fecha = e.Fecha,
                    Monto = e.Monto,
                    Estado = e.Estado
                }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarComoPagado(Guid id, CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;

            try
            {
                await _eventoService.MarcarComoPagadoAsync(tenantId, id, ct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}