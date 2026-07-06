using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Interfaces;
using Front.Models.Dashboard;
using Front.Models.Eventos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Front.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ICurrentTenantService _currentTenantService;

        public DashboardController(
            IDashboardService dashboardService,
            ICurrentTenantService currentTenantService)
        {
            _dashboardService = dashboardService;
            _currentTenantService = currentTenantService;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            var tenantId = _currentTenantService.TenantId;

            var dashboard = await _dashboardService.GetDashboardStatsAsync(
                tenantId,
                startDate,
                endDate);

            var model = new DashboardViewModel
            {
                TotalClientes = dashboard.TotalClientes,
                TotalEventos = dashboard.TotalEventos,
                IngresosTotales = dashboard.IngresosTotales,
                EventosPendientes = dashboard.EventosPendientes,
                EventosCompletados = dashboard.EventosCompletados,
                RecordatoriosEnviados = dashboard.RecordatoriosEnviados,
                RecordatoriosPendientes = dashboard.RecordatoriosPendientes,
                RecordatoriosFallidos = dashboard.RecordatoriosFallidos,

                UltimosEventos = dashboard.UltimosEventos
                    .Select(e => new RecentEventoViewModel
                    {
                        Id = e.EventoId,
                        ClienteNombre = e.ClienteNombre,
                        Fecha = e.Fecha,
                        Monto = e.Monto,
                        Estado = e.Estado,
                        Tipo = e.Tipo
                    })
                    .ToList()
            };

            return View(model);
        }
    }
}