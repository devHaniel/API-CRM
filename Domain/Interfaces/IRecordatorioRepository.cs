using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IRecordatorioRepository
    {
        Task<Recordatorio?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Recordatorio>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(Recordatorio recordatorio, CancellationToken ct = default);
        void Update(Recordatorio recordatorio);
        void Remove(Recordatorio recordatorio);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task<IEnumerable<Recordatorio>> GetPendientesDeEnvioAsync(CancellationToken ct = default);
    }
}
