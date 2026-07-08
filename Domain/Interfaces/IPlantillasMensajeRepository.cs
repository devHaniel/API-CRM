using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IPlantillasMensajeRepository
    {
        Task<PlantillasMensaje?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
        Task<IEnumerable<PlantillasMensaje>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct = default);
        Task<IEnumerable<PlantillasMensaje>> GetPagedByTenantAsync(Guid tenantId, int pageNumber, int pageSize, CancellationToken ct = default);
        Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default);
        Task AddAsync(PlantillasMensaje plantilla, CancellationToken ct = default);
        void Update(PlantillasMensaje plantilla);
        void Remove(PlantillasMensaje plantilla);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
