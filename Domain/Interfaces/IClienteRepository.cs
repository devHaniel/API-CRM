using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IClienteRepository
    {
        Task<Cliente?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
        Task<IEnumerable<Cliente>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct = default);
        Task<IEnumerable<Cliente>> GetPagedByTenantAsync(Guid tenantId, int pageNumber, int pageSize, CancellationToken ct = default);
        Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default);
        Task AddAsync(Cliente cliente, CancellationToken ct = default);
        void Update(Cliente cliente);
        void Remove(Cliente cliente);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task<bool> ExisteTelefonoAsync(Guid tenantId, string? telefono,  CancellationToken ct = default);
        Task<bool> ExisteEmailAsync(Guid tenantId, string? email, CancellationToken ct = default);
    }
}
