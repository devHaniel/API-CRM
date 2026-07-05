using System;
using System.Threading.Tasks;
using Application.DTOs.Dashboard;

namespace Application.Interfaces
{
    public interface IDashboardService
    {
        Task<TenantDashboardDto> GetDashboardStatsAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
