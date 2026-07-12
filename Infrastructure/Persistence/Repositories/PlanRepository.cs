using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
     public class PlanRepository : IPlanRepository
    {
        private readonly RecordAppDbContext _context;

        public PlanRepository(RecordAppDbContext context)
        {
            _context = context;
        }

        public async Task<Plane?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Planes.FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<IEnumerable<Plane>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
            => await _context.Planes
                .OrderBy(p => p.PrecioMensual)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

        public async Task<int> CountAsync(CancellationToken ct = default)
            => await _context.Planes.CountAsync(ct);

        public async Task<bool> ExisteNombreAsync(string nombre, Guid? excluirId = null, CancellationToken ct = default)
            => await _context.Planes.AnyAsync(p =>
                p.Nombre == nombre && (excluirId == null || p.Id != excluirId), ct);

        public async Task AddAsync(Plane plan, CancellationToken ct = default)
            => await _context.Planes.AddAsync(plan, ct);

        public void Update(Plane plan) => _context.Planes.Update(plan);

        public void Remove(Plane plan) => _context.Planes.Remove(plan);

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);

        public async Task<Plane?> ObtenerPlanDelTenantAsync(Guid tenantId, CancellationToken ct = default)
            => await _context.Tenants
                .Where(t => t.Id == tenantId)
                .Select(t => t.Plan)
                .FirstOrDefaultAsync(ct);

        public async Task AsignarPlanAlTenantAsync(Guid tenantId, Guid planId, CancellationToken ct = default)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, ct)
                ?? throw new KeyNotFoundException("Tenant no encontrado.");

            tenant.PlanId = planId;
            _context.Tenants.Update(tenant);
        }

        public async Task<int> ContarRecordatoriosDelMesAsync(Guid tenantId, CancellationToken ct = default)
        {
            var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            return await _context.Recordatorios
                .Where(r => r.Evento.TenantId == tenantId && r.FechaCreacion >= inicioMes)
                .CountAsync(ct);
        }

        public async Task<int> ContarUsuariosAsync(Guid tenantId, CancellationToken ct = default)
        {
            return await _context.Usuarios
                .Where(u => u.TenantId == tenantId)
                .CountAsync(ct);

        }
    }
}