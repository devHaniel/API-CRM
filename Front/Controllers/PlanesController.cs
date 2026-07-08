using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Plan;
using Application.Interfaces;
using Front.Models.Planes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Front.Controllers
{
     [Authorize]
    [Authorize(Roles = "Admin")]
    public class PlanesController : Controller
    {
        private readonly IPlanService _planService;

        public PlanesController(IPlanService planService)
        {
            _planService = planService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var resultado = await _planService.ObtenerTodosAsync(1, 100, ct);

            IEnumerable<PlanDto> model = resultado.Items;

            return View(model);
        }

        // public IActionResult Create()
        // {
        //     return View(new PlanFormViewModel());
        // }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create(PlanFormViewModel model, CancellationToken ct)
        // {
        //     if (!ModelState.IsValid)
        //         return View(model);

        //     try
        //     {
        //         var dto = new CrearPlanDto(
        //             model.Nombre,
        //             model.PrecioMensual,
        //             model.LimiteRecordatoriosMes,
        //             model.PrecioRecordatorioExtra,
        //             model.MaxUsuarios
        //         );

        //         await _planService.CrearAsync(dto, ct);

        //         return RedirectToAction(nameof(Index));
        //     }
        //     catch (InvalidOperationException ex)
        //     {
        //         ModelState.AddModelError(string.Empty, ex.Message);
        //         return View(model);
        //     }
        // }

        // public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        // {
        //     var plan = await _planService.ObtenerPorIdAsync(id, ct);

        //     if (plan is null)
        //         return NotFound();

        //     var model = new PlanFormViewModel
        //     {
        //         Id = plan.Id,
        //         Nombre = plan.Nombre,
        //         PrecioMensual = plan.PrecioMensual,
        //         LimiteRecordatoriosMes = plan.LimiteRecordatoriosMes,
        //         PrecioRecordatorioExtra = plan.PrecioRecordatorioExtra,
        //         MaxUsuarios = plan.MaxUsuarios,
        //         Activo = plan.Activo
        //     };

        //     return View(model);
        // }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Edit(PlanFormViewModel model, CancellationToken ct)
        // {
        //     if (!ModelState.IsValid)
        //         return View(model);

        //     try
        //     {
        //         var dto = new ActualizarPlanDto(
        //             model.Nombre,
        //             model.PrecioMensual,
        //             model.LimiteRecordatoriosMes,
        //             model.PrecioRecordatorioExtra,
        //             model.MaxUsuarios,
        //             model.Activo
        //         );

        //         await _planService.ActualizarAsync(model.Id, dto, ct);

        //         return RedirectToAction(nameof(Index));
        //     }
        //     catch (Exception ex) when (
        //         ex is InvalidOperationException ||
        //         ex is KeyNotFoundException)
        //     {
        //         ModelState.AddModelError(string.Empty, ex.Message);
        //         return View(model);
        //     }
        // }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        // {
        //     try
        //     {
        //         await _planService.EliminarAsync(id, ct);
        //     }
        //     catch (KeyNotFoundException)
        //     {
        //         return NotFound();
        //     }

        //     return RedirectToAction(nameof(Index));
        // }
    }
}