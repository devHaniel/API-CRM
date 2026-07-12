using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Common;
using Application.DTOs.Plan;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _planRepository;
        private readonly ILogger<PlanService> _logger;

        public PlanService(IPlanRepository planRepository, ILogger<PlanService> logger)
        {
            _planRepository = planRepository;
            _logger = logger;
        }

        public async Task<Guid> CrearAsync(CrearPlanDto dto, CancellationToken ct = default)
        {
            _logger.LogInformation("Creando plan {Nombre}.", dto.Nombre);

            if (await _planRepository.ExisteNombreAsync(dto.Nombre, null, ct))
            {
                _logger.LogWarning("No se pudo crear el plan. Ya existe un plan con el nombre {Nombre}.", dto.Nombre);
                throw new InvalidOperationException("Ya existe un plan con ese nombre.");
            }

            var plan = new Plane
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                PrecioMensual = dto.PrecioMensual,
                LimiteRecordatoriosMes = dto.LimiteRecordatoriosMes,
                PrecioRecordatorioExtra = dto.PrecioRecordatorioExtra,
                MaxUsuarios = dto.MaxUsuarios,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            await _planRepository.AddAsync(plan, ct);
            await _planRepository.SaveChangesAsync(ct);

            _logger.LogInformation("Plan creado exitosamente con ID {PlanId}.", plan.Id);
            return plan.Id;
        }

        public async Task<PlanDto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        {
            var plan = await _planRepository.GetByIdAsync(id, ct);
            return plan is null ? null : MapToDto(plan);
        }

        public async Task<PagedResultDto<PlanDto>> ObtenerTodosAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            _logger.LogInformation("Consultando lista paginada de planes. Página: {PageNumber}, Tamaño: {PageSize}", pageNumber, pageSize);

            var totalCount = await _planRepository.CountAsync(ct);
            var planes = await _planRepository.GetPagedAsync(pageNumber, pageSize, ct);

            var items = planes.Select(MapToDto);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PagedResultDto<PlanDto>(items, pageNumber, pageSize, totalCount, totalPages);
        }

        public async Task ActualizarAsync(Guid id, ActualizarPlanDto dto, CancellationToken ct = default)
        {
            var plan = await _planRepository.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Plan no encontrado.");

            if (dto.Nombre != plan.Nombre && await _planRepository.ExisteNombreAsync(dto.Nombre, id, ct))
                throw new InvalidOperationException("Ya existe un plan con ese nombre.");

            plan.Nombre = dto.Nombre;
            plan.PrecioMensual = dto.PrecioMensual;
            plan.LimiteRecordatoriosMes = dto.LimiteRecordatoriosMes;
            plan.PrecioRecordatorioExtra = dto.PrecioRecordatorioExtra;
            plan.MaxUsuarios = dto.MaxUsuarios;
            plan.Activo = dto.Activo;

            _planRepository.Update(plan);
            await _planRepository.SaveChangesAsync(ct);
        }

        public async Task EliminarAsync(Guid id, CancellationToken ct = default)
        {
            var plan = await _planRepository.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Plan no encontrado.");

            _planRepository.Remove(plan);
            await _planRepository.SaveChangesAsync(ct);
        }

        public async Task<TenantPlanDto> ObtenerPlanDelTenantAsync(Guid tenantId, CancellationToken ct = default)
        {
            var plan = await _planRepository.ObtenerPlanDelTenantAsync(tenantId, ct)
                ?? throw new InvalidOperationException("El tenant no tiene un plan asignado.");

            var uso = await _planRepository.ContarRecordatoriosDelMesAsync(tenantId, ct);

            return new TenantPlanDto(
                plan.Id,
                plan.Nombre,
                plan.PrecioMensual,
                plan.LimiteRecordatoriosMes,
                plan.PrecioRecordatorioExtra,
                plan.MaxUsuarios,
                uso
            );
        }

        public async Task CambiarPlanDelTenantAsync(Guid tenantId, Guid nuevoPlanId, CancellationToken ct = default)
        {
            var planNuevo = await _planRepository.GetByIdAsync(nuevoPlanId, ct)
                ?? throw new KeyNotFoundException("Plan no encontrado.");

            if (!planNuevo.Activo)
                throw new InvalidOperationException("El plan seleccionado no está disponible.");

            await _planRepository.AsignarPlanAlTenantAsync(tenantId, nuevoPlanId, ct);
            await _planRepository.SaveChangesAsync(ct);

            _logger.LogInformation("Tenant {TenantId} cambió al plan {PlanId}.", tenantId, nuevoPlanId);
        }

        public async Task<bool> PuedeEnviarMasRecordatoriosAsync(Guid tenantId, CancellationToken ct = default)
        {
            var plan = await _planRepository.ObtenerPlanDelTenantAsync(tenantId, ct);
            if (plan is null)
                return false;

            // Si el plan permite excedentes pagados, siempre puede seguir enviando
            if (plan.PrecioRecordatorioExtra > 0)
                return true;

            var uso = await _planRepository.ContarRecordatoriosDelMesAsync(tenantId, ct);
            return uso < plan.LimiteRecordatoriosMes;
        }

        public async Task<bool> PuedeCrearUsuariosAsync(Guid tenantId, CancellationToken ct = default)
        {
            var plan = await _planRepository.ObtenerPlanDelTenantAsync(tenantId, ct);
            if (plan is null)
                return false;

            var totalUsuarios = await _planRepository.ContarUsuariosAsync(tenantId, ct);
            return totalUsuarios < plan.MaxUsuarios;
        }

        private static PlanDto MapToDto(Plane p)
            => new(p.Id, p.Nombre, p.PrecioMensual, p.LimiteRecordatoriosMes, p.PrecioRecordatorioExtra, p.MaxUsuarios, p.Activo, p.FechaCreacion);
    }
}