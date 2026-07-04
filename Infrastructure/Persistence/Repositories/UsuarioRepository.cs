using Domain;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly RecordAppDbContext _context;

        public UsuarioRepository(RecordAppDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

        public async Task<IEnumerable<Usuario>> GetAllAsync(CancellationToken ct = default)
            => await _context.Usuarios.ToListAsync(ct);

        public async Task AddAsync(Usuario usuario, CancellationToken ct = default)
            => await _context.Usuarios.AddAsync(usuario, ct);

        public void Update(Usuario usuario) => _context.Usuarios.Update(usuario);

        public void Remove(Usuario usuario) => _context.Usuarios.Remove(usuario);

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);

        public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default)
            => await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);

        public async Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default)
            => await _context.Usuarios.AnyAsync(u => u.Email == email, ct);
    }
}
