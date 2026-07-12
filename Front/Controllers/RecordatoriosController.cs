using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Recordatorio;
using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Front.Models.Recordatorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace Front.Controllers
{
    [Authorize]
    public class RecordatoriosController : Controller
    {
        private readonly IRecordatorioService _recordatorioService;
        private readonly IEventoService _eventoService;
        private readonly IPlantillasMensajeService _plantillaService;
        private readonly ICurrentTenantService _currentTenantService;

        public RecordatoriosController(
            IRecordatorioService recordatorioService,
            IEventoService eventoService,
            IPlantillasMensajeService plantillaService,
            ICurrentTenantService currentTenantService)
        {
            _recordatorioService = recordatorioService;
            _eventoService = eventoService;
            _plantillaService = plantillaService;
            _currentTenantService = currentTenantService;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var tenantId = _currentTenantService.TenantId;
            var resultado = await _recordatorioService.ObtenerTodosAsync(tenantId, pageNumber, pageSize, ct);
            return View(resultado);
        }

        public async Task<IActionResult> Details(Guid id, CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;
            var recordatorio = await _recordatorioService.ObtenerPorIdAsync(tenantId, id, ct);

            if (recordatorio is null)
                return NotFound();

            var model = new RecordatorioDetailsViewModel
            {
                Id = recordatorio.Id,
                EventoId = recordatorio.EventoId,
                EventoTipo = recordatorio.EventoTipo,
                ClienteId = recordatorio.ClienteId,
                ClienteNombre = recordatorio.ClienteNombre,
                PlantillaId = recordatorio.PlantillaId,
                CanalEnvio = recordatorio.CanalEnvio,
                FechaProgramada = recordatorio.FechaProgramada,
                FechaEnvio = recordatorio.FechaEnvio,
                Estado = recordatorio.Estado,
                DetalleError = recordatorio.DetalleError,
                FechaCreacion = recordatorio.FechaCreacion
            };

            return View(model);
        }

        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var model = new RecordatorioFormViewModel();
            await CargarListasAsync(model, ct);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecordatorioFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await CargarListasAsync(model, ct);
                return View(model);
            }

            var tenantId = _currentTenantService.TenantId;

            try
            {
                var dto = new CrearRecordatorioDto(
                    model.EventoId,
                    model.PlantillaId,
                    model.CanalEnvio,
                    model.FechaProgramada,
                    "Pendiente"
                );

                await _recordatorioService.CrearAsync(tenantId, dto, ct);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await CargarListasAsync(model, ct);
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
                await _recordatorioService.EliminarAsync(tenantId, id, ct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;
            var recordatorio = await _recordatorioService.ObtenerPorIdAsync(tenantId, id, ct);

            if (recordatorio is null)
                return NotFound();

            var model = new RecordatorioFormViewModel
            {
                Id = recordatorio.Id,
                EventoId = recordatorio.EventoId,
                PlantillaId = recordatorio.PlantillaId ?? Guid.Empty,
                CanalEnvio = recordatorio.CanalEnvio,
                FechaProgramada = recordatorio.FechaProgramada
            };

            await CargarListasAsync(model, ct);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RecordatorioFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await CargarListasAsync(model, ct);
            return View(model);
        }

        var tenantId = _currentTenantService.TenantId;

        try
        {
            var dto = new ActualizarRecordatorioDto(
                model.EventoId,
                model.PlantillaId,
                model.CanalEnvio,
                model.FechaProgramada,
                null,              // FechaEnvio — no se edita manualmente aquí
                "Pendiente",       // Estado — vuelve a Pendiente si se reprograma
                null               // DetalleError — se limpia al editar
            );

            await _recordatorioService.ActualizarAsync(tenantId, model.Id, dto, ct);
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await CargarListasAsync(model, ct);
            return View(model);
        }
    }

        private async Task CargarListasAsync(RecordatorioFormViewModel model, CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;

            var eventos = await _eventoService.ObtenerTodosAsync(tenantId, 1, 500, ct);
            var plantillas = await _plantillaService.ObtenerTodosAsync(tenantId);

            model.EventosDisponibles = eventos.Items.Select(e => new SelectListItem(
                $"{e.ClienteNombre} - {e.Tipo} ({e.Fecha:dd/MM/yyyy HH:mm})", e.Id.ToString()
            )).ToList();

            model.PlantillasDisponibles = plantillas.Items.Select(p =>
                new SelectListItem(p.Tipo, p.Id.ToString())
            ).ToList();

            model.EventosData = eventos.Items.Select(e => new EventoPreviewData
            {
                Id = e.Id,
                ClienteNombre = e.ClienteNombre ?? "cliente",
                Tipo = e.Tipo,
                Descripcion = e.Descripcion,
                Fecha = e.Fecha
            }).ToList();

            model.PlantillasData = plantillas.Items.Select(p => new PlantillaPreviewData
            {
                Id = p.Id,
                Contenido = p.Contenido
            }).ToList();
        }
    }
}