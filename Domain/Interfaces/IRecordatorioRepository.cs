using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IRecordatorioRepository
    {
        Task<Recordatorio?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
        Task<Recordatorio?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Recordatorio>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct = default);
        Task<IEnumerable<Recordatorio>> GetPagedByTenantAsync(Guid tenantId, int pageNumber, int pageSize, CancellationToken ct = default);
        Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default);
        Task AddAsync(Recordatorio recordatorio, CancellationToken ct = default);
        void Update(Recordatorio recordatorio);
        void Remove(Recordatorio recordatorio);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task<bool> PlantillaPerteneceAlTenantAsync(Guid tenantId, Guid plantillaId, CancellationToken ct = default);
        Task<IEnumerable<Recordatorio>> GetPendientesDeEnvioAsync(Guid tenantId, CancellationToken ct = default);
        Task<IEnumerable<Recordatorio>> GetPendientesDeEnvioAsync(CancellationToken ct = default);
    }
}
