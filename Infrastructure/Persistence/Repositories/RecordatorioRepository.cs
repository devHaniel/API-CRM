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

        public async Task<Recordatorio?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
            => await _context.Recordatorios
                .Include(r => r.Evento)
                .ThenInclude(e => e.Cliente)
                .Include(r => r.Plantilla)
                .FirstOrDefaultAsync(r => r.Id == id && r.Evento.TenantId == tenantId, ct);

        public async Task<Recordatorio?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Recordatorios
                .Include(r => r.Evento)
                .ThenInclude(e => e.Cliente)
                .Include(r => r.Plantilla)
                .FirstOrDefaultAsync(r => r.Id == id, ct);

        public async Task<IEnumerable<Recordatorio>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct = default)
            => await _context.Recordatorios
                .Where(r => r.Evento.TenantId == tenantId)
                .Include(r => r.Evento)
                .ThenInclude(e => e.Cliente)
                .Include(r => r.Plantilla)
                .OrderBy(r => r.FechaProgramada)
                .ToListAsync(ct);

        public async Task AddAsync(Recordatorio recordatorio, CancellationToken ct = default)
            => await _context.Recordatorios.AddAsync(recordatorio, ct);

        public void Update(Recordatorio recordatorio) => _context.Recordatorios.Update(recordatorio);

        public void Remove(Recordatorio recordatorio) => _context.Recordatorios.Remove(recordatorio);

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);

        public async Task<bool> PlantillaPerteneceAlTenantAsync(Guid tenantId, Guid plantillaId, CancellationToken ct = default)
            => await _context.PlantillasMensajes.AnyAsync(p => p.Id == plantillaId && p.TenantId == tenantId, ct);

        public async Task<IEnumerable<Recordatorio>> GetPendientesDeEnvioAsync(Guid tenantId, CancellationToken ct = default)
            => await _context.Recordatorios
                .Where(r => r.Evento.TenantId == tenantId && r.Estado == "Pendiente" && r.FechaProgramada <= DateTime.UtcNow)
                .Include(r => r.Evento)
                .ThenInclude(e => e.Cliente)
                .Include(r => r.Plantilla)
                .ToListAsync(ct);

        public async Task<IEnumerable<Recordatorio>> GetPendientesDeEnvioAsync(CancellationToken ct = default)
            => await _context.Recordatorios
                .Where(r => r.Estado == "Pendiente" && r.FechaProgramada <= DateTime.UtcNow)
                .Include(r => r.Evento)
                .ThenInclude(e => e.Cliente)
                .Include(r => r.Plantilla)
                .ToListAsync(ct);
    }
}
