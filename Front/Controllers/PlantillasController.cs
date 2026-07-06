using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.PlantillasMensaje;
using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Front.Models.Plantillas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Front.Controllers
{
      [Authorize]
    public class PlantillasController : Controller
    {
        private readonly IPlantillasMensajeService _plantillaService;
        private readonly ICurrentTenantService _currentTenantService;

        public PlantillasController(
            IPlantillasMensajeService plantillaService,
            ICurrentTenantService currentTenantService)
        {
            _plantillaService = plantillaService;
            _currentTenantService = currentTenantService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;
            var plantillas = await _plantillaService.ObtenerTodosAsync(tenantId);

            var model = plantillas.Items.Select(p => new PlantillaListItemViewModel
            {
                Id = p.Id,
                Tipo = p.Tipo,
                Contenido = p.Contenido,
                Activo = p.Activo
            }).ToList();

            return View(model);
        }

        public IActionResult Create()
        {
            return View(new PlantillaFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlantillaFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tenantId = _currentTenantService.TenantId;

            try
            {
                var dto = new CrearPlantillasMensajeDto(model.Tipo, model.Contenido, model.Activo);
                await _plantillaService.CrearAsync(tenantId, dto, ct);

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;
            var plantilla = await _plantillaService.ObtenerPorIdAsync(tenantId, id, ct);

            if (plantilla is null)
                return NotFound();

            var model = new PlantillaFormViewModel
            {
                Id = plantilla.Id,
                Tipo = plantilla.Tipo,
                Contenido = plantilla.Contenido,
                Activo = plantilla.Activo
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PlantillaFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tenantId = _currentTenantService.TenantId;

            try
            {
                var dto = new ActualizarPlantillasMensajeDto(model.Tipo, model.Contenido, model.Activo);
                await _plantillaService.ActualizarAsync(tenantId, model.Id, dto, ct);

                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
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
                await _plantillaService.EliminarAsync(tenantId, id, ct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}