using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Dashboard;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly RecordAppDbContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(RecordAppDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TenantDashboardDto> GetDashboardStatsAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            // Obtener conteo de clientes activos del Tenant
            var totalClientes = await _context.Clientes
                .CountAsync(c => c.TenantId == tenantId && c.Activo);

            // Consulta base de eventos del Tenant en el rango de fechas
            var queryEventos = _context.Eventos
                .Where(e => e.TenantId == tenantId);

            var totalEventos = await queryEventos.CountAsync();
            
            // Suma de ingresos de los eventos pagados
            var ingresosTotales = await queryEventos
                .Where(e => e.Estado == "Pagado" && e.Monto != null)
                .SumAsync(e => e.Monto ?? 0);

            _logger.LogInformation($"Ingresos: {ingresosTotales}, Total Eventos: {totalEventos}, Total Clientes: {totalClientes}");

            var eventosPendientes = await queryEventos
                .CountAsync(e => e.Estado == "Pendiente");

            var eventosCompletados = await queryEventos
                .CountAsync(e => e.Estado == "Completado");

            // Métricas de recordatorios asociados a los eventos del tenant
            var queryRecordatorios = _context.Recordatorios
                .Where(r => r.Evento.TenantId == tenantId);

            var recordatoriosEnviados = await queryRecordatorios
                .CountAsync(r => r.Estado == "Enviado");

            var recordatoriosPendientes = await queryRecordatorios
                .CountAsync(r => r.Estado == "Pendiente");

            var recordatoriosFallidos = await queryRecordatorios
                .CountAsync(r => r.Estado == "Fallido");

            // Obtener los 5 eventos más recientes
            var ultimosEventos = await _context.Eventos
                .Include(e => e.Cliente)
                .Where(e => e.TenantId == tenantId)
                .OrderByDescending(e => e.Fecha)
                .Take(5)
                .Select(e => new RecentEventoDto(
                    e.Id,
                    e.Cliente != null ? e.Cliente.Nombre : "Sin Cliente",
                    e.Fecha,
                    e.Monto ?? 0,
                    e.Estado ?? "Pendiente",
                    e.Tipo ?? ""
                ))
                .ToListAsync();

            return new TenantDashboardDto(
                totalClientes,
                totalEventos,
                ingresosTotales,
                eventosPendientes,
                eventosCompletados,
                recordatoriosEnviados,
                recordatoriosPendientes,
                recordatoriosFallidos,
                ultimosEventos
            );
        }
    }
}
