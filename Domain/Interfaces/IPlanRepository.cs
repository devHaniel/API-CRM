using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IPlanRepository
    {
        Task<Plane?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Plane>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
        Task<int> CountAsync(CancellationToken ct = default);
        Task<bool> ExisteNombreAsync(string nombre, Guid? excluirId = null, CancellationToken ct = default);

        Task AddAsync(Plane plan, CancellationToken ct = default);
        void Update(Plane plan);
        void Remove(Plane plan);
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        // Específicos de la relación Plan <-> Tenant
        Task<Plane?> ObtenerPlanDelTenantAsync(Guid tenantId, CancellationToken ct = default);
        Task AsignarPlanAlTenantAsync(Guid tenantId, Guid planId, CancellationToken ct = default);
        Task<int> ContarRecordatoriosDelMesAsync(Guid tenantId, CancellationToken ct = default);
    }
}