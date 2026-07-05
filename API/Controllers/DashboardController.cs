using System;
using System.Threading.Tasks;
using Application.DTOs.Dashboard;
using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ICurrentTenantService _tenantService;

        public DashboardController(IDashboardService dashboardService, ICurrentTenantService tenantService)
        {
            _dashboardService = dashboardService;
            _tenantService = tenantService;
        }

        [HttpGet]
        public async Task<ActionResult<TenantDashboardDto>> GetDashboard(
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate)
        {
            var tenantId = _tenantService.TenantId;
            var stats = await _dashboardService.GetDashboardStatsAsync(tenantId, startDate, endDate);
            return Ok(stats);
        }
    }
}
