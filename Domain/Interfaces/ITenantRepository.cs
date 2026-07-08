using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ITenantRepository
    {
        Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(Tenant tenant, CancellationToken ct = default);
        void Update(Tenant tenant);
        void Remove(Tenant tenant);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
