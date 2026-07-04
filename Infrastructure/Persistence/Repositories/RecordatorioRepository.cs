using Domain;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class RecordatorioRepository : IRecordatorioRepository
    {
        private readonly RecordAppDbContext _context;

        public RecordatorioRepository(RecordAppDbContext context)
        {
            _context = context;
        }

        public async Task<Recordatorio?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Recordatorios.FirstOrDefaultAsync(r => r.Id == id, ct);

        public async Task<IEnumerable<Recordatorio>> GetAllAsync(CancellationToken ct = default)
            => await _context.Recordatorios.ToListAsync(ct);

        public async Task AddAsync(Recordatorio recordatorio, CancellationToken ct = default)
            => await _context.Recordatorios.AddAsync(recordatorio, ct);

        public void Update(Recordatorio recordatorio) => _context.Recordatorios.Update(recordatorio);

        public void Remove(Recordatorio recordatorio) => _context.Recordatorios.Remove(recordatorio);

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);

        public async Task<IEnumerable<Recordatorio>> GetPendientesDeEnvioAsync(CancellationToken ct = default)
            => await _context.Recordatorios
                .Where(r => r.Estado == "Pendiente" && r.FechaProgramada <= DateTime.UtcNow)
                .Include(r => r.Evento)
                .ThenInclude(e => e.Cliente)
                .ToListAsync(ct);
    }
}
