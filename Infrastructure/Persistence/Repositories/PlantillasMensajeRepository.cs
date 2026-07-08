using Domain;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class PlantillasMensajeRepository : IPlantillasMensajeRepository
    {
        private readonly RecordAppDbContext _context;

        public PlantillasMensajeRepository(RecordAppDbContext context)
        {
            _context = context;
        }

        public async Task<PlantillasMensaje?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
            => await _context.PlantillasMensajes
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, ct);

        public async Task<IEnumerable<PlantillasMensaje>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct = default)
            => await _context.PlantillasMensajes
                .Where(p => p.TenantId == tenantId)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync(ct);

        public async Task<IEnumerable<PlantillasMensaje>> GetPagedByTenantAsync(Guid tenantId, int pageNumber, int pageSize, CancellationToken ct = default)
            => await _context.PlantillasMensajes
                .Where(p => p.TenantId == tenantId)
                .OrderByDescending(p => p.FechaCreacion)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

        public async Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default)
            => await _context.PlantillasMensajes.CountAsync(p => p.TenantId == tenantId, ct);

        public async Task AddAsync(PlantillasMensaje plantilla, CancellationToken ct = default)
            => await _context.PlantillasMensajes.AddAsync(plantilla, ct);

        public void Update(PlantillasMensaje plantilla) => _context.PlantillasMensajes.Update(plantilla);

        public void Remove(PlantillasMensaje plantilla) => _context.PlantillasMensajes.Remove(plantilla);

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);
    }
}
