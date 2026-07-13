using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
        Task<IEnumerable<Usuario>> GetAllAsync(Guid tenantId, CancellationToken ct = default);
        Task<Usuario?> GetAdminByTenantAsync(Guid tenantId, CancellationToken ct = default);
        Task AddAsync(Usuario usuario, CancellationToken ct = default);
        void Update(Usuario usuario);
        void Remove(Usuario usuario);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default);
    }   
}
