using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Front.Models.Shared
{
    public class PlanCardViewComponent : ViewComponent
{
    private readonly IPlanService _planService;
    private readonly ICurrentTenantService _currentTenantService;

    public PlanCardViewComponent(
        IPlanService planService,
        ICurrentTenantService currentTenantService)
    {
        _planService = planService;
        _currentTenantService = currentTenantService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var tenantId = _currentTenantService.TenantId;

        var plan = await _planService.ObtenerPlanDelTenantAsync(tenantId);

        var model = new PlanCardViewModel
        {
            Nombre = plan.Nombre,
            PrecioMensual = plan.PrecioMensual,
            LimiteRecordatoriosMes = plan.LimiteRecordatoriosMes,
            RecordatoriosUsados = plan.UsoActualMes
        };

        return View(model);
    }
}
}