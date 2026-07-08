using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IEventoRepository
    {
        Task<Evento?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
        Task<IEnumerable<Evento>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct = default);
        Task<List<Evento>?> GetByIdClienteAsync(Guid id, Guid tenantId, CancellationToken ct = default);
        Task<IEnumerable<Evento>> GetPagedByTenantAsync(Guid tenantId, int pageNumber, int pageSize, CancellationToken ct = default);
        Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default);
        Task AddAsync(Evento evento, CancellationToken ct = default);
        void Update(Evento evento);
        void Remove(Evento evento);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task<IEnumerable<Evento>> GetProximosAsync(Guid tenantId, DateTime desde, DateTime hasta, CancellationToken ct = default);
        Task<IEnumerable<Evento>> GetPendientesDePagoAsync(Guid tenantId, CancellationToken ct = default);
    }
}
