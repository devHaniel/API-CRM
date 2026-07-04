using Domain;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly RecordAppDbContext _context;

        public TenantRepository(RecordAppDbContext context)
        {
            _context = context;
        }

        public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);

        public async Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken ct = default)
            => await _context.Tenants.ToListAsync(ct);

        public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
            => await _context.Tenants.AddAsync(tenant, ct);

        public void Update(Tenant tenant) => _context.Tenants.Update(tenant);

        public void Remove(Tenant tenant) => _context.Tenants.Remove(tenant);

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);
    }
}
