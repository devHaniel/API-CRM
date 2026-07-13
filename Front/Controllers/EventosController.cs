using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Application.DTOs.Evento;
using Application.Interfaces;
using Domain.Interfaces;
using Front.Models.Eventos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

using Front.Models.Recordatorios;

namespace Front.Controllers
{
    public record CrearDesdeCalendarioRequest(
        Guid ClienteId,
        DateTime Fecha,
        string? Descripcion,
        decimal? Monto
    );

    [Authorize]
    public class EventosController : Controller
    {
        private readonly IEventoService _eventoService;
        private readonly IClienteService _clienteService;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly IRecordatorioService _recordatorioService;

        public EventosController(
            IEventoService eventoService,
            IClienteService clienteService,
            ICurrentTenantService currentTenantService,
            IRecordatorioService recordatorioService)
        {
            _eventoService = eventoService;
            _clienteService = clienteService;
            _currentTenantService = currentTenantService;
            _recordatorioService = recordatorioService;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var tenantId = _currentTenantService.TenantId;

            var resultado = await _eventoService.ObtenerTodosAsync(tenantId, pageNumber, pageSize, ct);

            var model = new EventosIndexViewModel
            {
                Eventos = resultado.Items.Select(e => new EventoListItemViewModel
                {
                    Id = e.Id,
                    ClienteNombre = e.ClienteNombre,
                    Tipo = e.Tipo,
                    Fecha = e.Fecha,
                    Descripcion = e.Descripcion,
                    Monto = e.Monto,
                    Estado = e.Estado
                }).ToList(),
                PageNumber = resultado.PageNumber,
                PageSize = resultado.PageSize,
                TotalCount = resultado.TotalCount,
                TotalPages = resultado.TotalPages
            };

            return View(model);
        }

        public async Task<IActionResult> Details(Guid id, CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;

            var evento = await _eventoService.ObtenerPorIdAsync(tenantId, id, ct);
            if (evento is null)
                return NotFound();

            var todos = await _recordatorioService.ObtenerTodosAsync(tenantId, 1, 1000, ct);
            var recordatoriosEvento = todos.Items
                .Where(r => r.EventoId == id)
                .OrderByDescending(r => r.FechaCreacion)
                .ToList();

            var model = new EventoDetailsViewModel
            {
                Id = evento.Id,
                ClienteId = evento.ClienteId,
                ClienteNombre = evento.ClienteNombre,
                Tipo = evento.Tipo,
                Fecha = evento.Fecha,
                Descripcion = evento.Descripcion,
                Monto = evento.Monto,
                Estado = evento.Estado,
                FechaCreacion = evento.FechaCreacion,
                Recordatorios = recordatoriosEvento
            };

            return View(model);
        }

        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var model = new EventoFormViewModel
            {
                ClientesDisponibles = await ObtenerClientesSelectListAsync(ct)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventoFormViewModel model, CancellationToken ct)
        {
            if (model.Tipo == "Pago" && model.Monto is null)
                ModelState.AddModelError(nameof(model.Monto), "El monto es obligatorio para eventos de tipo Pago.");

            if (!ModelState.IsValid)
            {
                model.ClientesDisponibles = await ObtenerClientesSelectListAsync(ct);
                return View(model);
            }

            var tenantId = _currentTenantService.TenantId;

            try
            {
                var dto = new CrearEventoDto(
                    model.ClienteId,
                    model.Tipo,
                    model.Fecha,
                    model.Descripcion,
                    model.Monto,
                    model.Estado
                );

                await _eventoService.CrearAsync(tenantId, dto, ct);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.DataAnnotations.ValidationException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.ClientesDisponibles = await ObtenerClientesSelectListAsync(ct);
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;

            var evento = await _eventoService.ObtenerPorIdAsync(tenantId, id, ct);
            if (evento is null)
                return NotFound();

            var model = new EventoFormViewModel
            {
                Id = evento.Id,
                ClienteId = evento.ClienteId,
                Tipo = evento.Tipo,
                Fecha = evento.Fecha,
                Descripcion = evento.Descripcion,
                Monto = evento.Monto,
                Estado = evento.Estado,
                ClientesDisponibles = await ObtenerClientesSelectListAsync(ct)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EventoFormViewModel model, CancellationToken ct)
        {
            if (model.Tipo == "Pago" && model.Monto is null)
                ModelState.AddModelError(nameof(model.Monto), "El monto es obligatorio para eventos de tipo Pago.");

            if (!ModelState.IsValid)
            {
                model.ClientesDisponibles = await ObtenerClientesSelectListAsync(ct);
                return View(model);
            }

            var tenantId = _currentTenantService.TenantId;

            try
            {
                var dto = new ActualizarEventoDto(
                    model.ClienteId,
                    model.Tipo,
                    model.Fecha,
                    model.Monto,
                    model.Descripcion,
                    model.Estado
                );

                await _eventoService.ActualizarAsync(tenantId, model.Id, dto, ct);

                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.DataAnnotations.ValidationException)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.ClientesDisponibles = await ObtenerClientesSelectListAsync(ct);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;

            try
            {
                await _eventoService.EliminarAsync(tenantId, id, ct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Calendario(CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;
            var clientes = await _clienteService.ObtenerTodosAsync(tenantId, pageNumber: 1, pageSize: 500, ct);
            ViewBag.ClientesJson = JsonSerializer.Serialize(
                clientes.Items.Select(c => new { id = c.Id.ToString(), nombre = c.Nombre })
            );
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CrearDesdeCalendario([FromBody] CrearDesdeCalendarioRequest request, CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;

            try
            {
                var dto = new CrearEventoDto(
                    request.ClienteId,
                    "Cita",
                    request.Fecha,
                    request.Descripcion,
                    request.Monto,
                    "Pendiente"
                );

                var id = await _eventoService.CrearAsync(tenantId, dto, ct);
                return Ok(new { success = true, id });
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.DataAnnotations.ValidationException)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEventosCalendario(DateTime start, DateTime end, CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;
            var eventos = await _eventoService.ObtenerProximosAsync(tenantId, start, end, ct);

            var result = eventos.Select(e => new
            {
                id = e.Id.ToString(),
                title = $"{e.ClienteNombre} - {e.Tipo}",
                start = e.Fecha.ToString("o"),
                allDay = true,
                extendedProps = new
                {
                    clienteNombre = e.ClienteNombre,
                    tipo = e.Tipo,
                    monto = e.Monto,
                    descripcion = e.Descripcion,
                    estado = e.Estado
                }
            });

            return Ok(result);
        }

        private async Task<List<SelectListItem>> ObtenerClientesSelectListAsync(CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;

            // pageSize alto para traer "todos" los clientes en el dropdown
            var clientes = await _clienteService.ObtenerTodosAsync(tenantId, pageNumber: 1, pageSize: 500, ct);

            return clientes.Items
                .Select(c => new SelectListItem(c.Nombre, c.Id.ToString()))
                .ToList();
        }
    }
}