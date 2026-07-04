using Domain;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly RecordAppDbContext _context;

        public ClienteRepository(RecordAppDbContext context)
        {
            _context = context;
        }

        public async Task<Cliente?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
            => await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, ct);

        public async Task<IEnumerable<Cliente>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct = default)
            => await _context.Clientes.Where(c => c.TenantId == tenantId).ToListAsync(ct);

        public async Task AddAsync(Cliente cliente, CancellationToken ct = default)
            => await _context.Clientes.AddAsync(cliente, ct);

        public void Update(Cliente cliente) => _context.Clientes.Update(cliente);

        public void Remove(Cliente cliente) => _context.Clientes.Remove(cliente);

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);

        public async Task<bool> ExisteTelefonoAsync(Guid tenantId, string? telefono, CancellationToken ct = default)
            => await _context.Clientes.AnyAsync(c => c.Telefono == telefono && c.TenantId == tenantId, ct);

        public async Task<bool> ExisteEmailAsync(Guid tenantId, string? email, CancellationToken ct = default)
            => await _context.Clientes.AnyAsync(c => c.Email == email && c.TenantId == tenantId, ct);
    }
}
