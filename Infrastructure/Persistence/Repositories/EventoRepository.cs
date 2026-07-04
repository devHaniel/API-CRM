using Domain;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class EventoRepository : IEventoRepository
    {
        private readonly RecordAppDbContext _context;

        public EventoRepository(RecordAppDbContext context)
        {
            _context = context;
        }

        public async Task<Evento?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
            => await _context.Eventos.FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId, ct);

        public async Task<IEnumerable<Evento>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct = default)
            => await _context.Eventos.Where(e => e.TenantId == tenantId).ToListAsync(ct);

        public async Task AddAsync(Evento evento, CancellationToken ct = default)
            => await _context.Eventos.AddAsync(evento, ct);

        public void Update(Evento evento) => _context.Eventos.Update(evento);

        public void Remove(Evento evento) => _context.Eventos.Remove(evento);

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);

        public async Task<IEnumerable<Evento>> GetProximosAsync(Guid tenantId, DateTime desde, DateTime hasta, CancellationToken ct = default)
            => await _context.Eventos
                .Where(e => e.TenantId == tenantId && e.Fecha >= desde && e.Fecha <= hasta)
                .Include(e => e.Cliente)
                .OrderBy(e => e.Fecha)
                .ToListAsync(ct);

        public async Task<IEnumerable<Evento>> GetPendientesDePagoAsync(Guid tenantId, CancellationToken ct = default)
            => await _context.Eventos
                .Where(e => e.TenantId == tenantId && e.Tipo == "Pago" && e.Estado != "Pagado")
                .Include(e => e.Cliente)
                .ToListAsync(ct);
    }
}
