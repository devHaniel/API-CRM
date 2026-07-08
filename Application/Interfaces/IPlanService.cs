using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Common;
using Application.DTOs.Plan;

namespace Application.Interfaces
{
    public interface IPlanService
    {
        Task<Guid> CrearAsync(CrearPlanDto dto, CancellationToken ct = default);
        Task<PlanDto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
        Task<PagedResultDto<PlanDto>> ObtenerTodosAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task ActualizarAsync(Guid id, ActualizarPlanDto dto, CancellationToken ct = default);
        Task EliminarAsync(Guid id, CancellationToken ct = default);

        Task<TenantPlanDto> ObtenerPlanDelTenantAsync(Guid tenantId, CancellationToken ct = default);
        Task CambiarPlanDelTenantAsync(Guid tenantId, Guid nuevoPlanId, CancellationToken ct = default);
        Task<bool> PuedeEnviarMasRecordatoriosAsync(Guid tenantId, CancellationToken ct = default);
    }
}