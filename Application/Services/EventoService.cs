using System.ComponentModel.DataAnnotations;
using Application.DTOs.Common;
using Application.DTOs.Evento;
using Application.Interfaces;
using Domain;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class EventoService : IEventoService
    {
        private readonly IEventoRepository _eventoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly ILogger<EventoService> _logger;

        public EventoService(IEventoRepository eventoRepository, IClienteRepository clienteRepository, ILogger<EventoService> logger)
        {
            _eventoRepository = eventoRepository;
            _clienteRepository = clienteRepository;
            _logger = logger;
        }

        public async Task<Guid> CrearAsync(Guid tenantId, CrearEventoDto dto, CancellationToken ct = default)
        {
            _logger.LogInformation("Iniciando creación de evento tipo {Tipo} para el Tenant {TenantId}.", dto.Tipo, tenantId);
            
            try
            {
                await ValidarClienteDelTenantAsync(tenantId, dto.ClienteId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Validación de cliente fallida para el Tenant {TenantId}. Cliente: {ClienteId}.", tenantId, dto.ClienteId);
                throw;
            }

            if (dto.Tipo == "Pago" && dto.Monto is null)
            {
                _logger.LogWarning("Validación fallida: Monto es obligatorio para eventos tipo Pago.");
                throw new ValidationException("El monto es obligatorio cuando el tipo es 'Pago'.");
            }

            if (dto.Tipo != "Pago" && dto.Tipo != "Cita")
            {
                _logger.LogWarning("Validación fallida: Tipo de evento inválido '{Tipo}'.", dto.Tipo);
                throw new ValidationException($"Tipo de evento inválido: '{dto.Tipo}'.");
            }

            var evento = new Evento
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ClienteId = dto.ClienteId,
                Tipo = dto.Tipo,
                Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc),
                Descripcion = dto.Descripcion,
                Monto = dto.Monto,
                Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Pendiente" : dto.Estado,
                FechaCreacion = DateTime.UtcNow
            };

            await _eventoRepository.AddAsync(evento, ct);
            await _eventoRepository.SaveChangesAsync(ct);

            _logger.LogInformation("Evento creado exitosamente con ID {EventoId} para el Tenant {TenantId}.", evento.Id, tenantId);
            return evento.Id;
        }

        public async Task<EventoDto?> ObtenerPorIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation("Obteniendo evento {EventoId} para el Tenant {TenantId}.", id, tenantId);
            var evento = await _eventoRepository.GetByIdAsync(id, tenantId, ct);
            return evento is null ? null : MapToDto(evento);
        }

        public async Task<PagedResultDto<EventoDto>> ObtenerTodosAsync(Guid tenantId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            _logger.LogInformation("Consultando lista paginada de eventos para el Tenant {TenantId}. Página: {PageNumber}, Tamaño: {PageSize}", tenantId, pageNumber, pageSize);
            
            var totalCount = await _eventoRepository.CountByTenantAsync(tenantId, ct);
            var eventos = await _eventoRepository.GetPagedByTenantAsync(tenantId, pageNumber, pageSize, ct);
            
            var items = eventos.Select(MapToDto);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PagedResultDto<EventoDto>(items, pageNumber, pageSize, totalCount, totalPages);
        }

        public async Task<IEnumerable<EventoDto>> ObtenerProximosAsync(Guid tenantId, DateTime desde, DateTime hasta, CancellationToken ct = default)
        {
            if (hasta < desde)
                throw new ArgumentException("La fecha final no puede ser menor que la fecha inicial.");

            var eventos = await _eventoRepository.GetProximosAsync(tenantId, desde, hasta, ct);
            return eventos.Select(MapToDto);
        }

        public async Task<IEnumerable<EventoDto>> ObtenerPendientesDePagoAsync(Guid tenantId, CancellationToken ct = default)
        {
            var eventos = await _eventoRepository.GetPendientesDePagoAsync(tenantId, ct);
            return eventos.Select(MapToDto);
        }

        public async Task ActualizarAsync(Guid tenantId, Guid id, ActualizarEventoDto dto, CancellationToken ct = default)
        {
            var evento = await _eventoRepository.GetByIdAsync(id, tenantId, ct)
                ?? throw new KeyNotFoundException("Evento no encontrado.");

            await ValidarClienteDelTenantAsync(tenantId, dto.ClienteId, ct);

            evento.ClienteId = dto.ClienteId;
            evento.Tipo = dto.Tipo;
            evento.Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc);
            evento.Descripcion = dto.Descripcion;
            evento.Monto = dto.Monto;
            evento.Estado = dto.Estado;

            _eventoRepository.Update(evento);
            await _eventoRepository.SaveChangesAsync(ct);
        }

        public async Task EliminarAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            var evento = await _eventoRepository.GetByIdAsync(id, tenantId, ct)
                ?? throw new KeyNotFoundException("Evento no encontrado.");

            _eventoRepository.Remove(evento);
            await _eventoRepository.SaveChangesAsync(ct);
        }

        private async Task ValidarClienteDelTenantAsync(Guid tenantId, Guid clienteId, CancellationToken ct)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId, tenantId, ct);
            if (cliente is null)
                throw new InvalidOperationException("El cliente no existe o no pertenece al tenant actual.");
        }

        private static EventoDto MapToDto(Evento e)
            => new(
                e.Id,
                e.ClienteId,
                e.Cliente?.Nombre,
                e.Tipo,
                e.Fecha,
                e.Descripcion,
                e.Monto,
                e.Estado,
                e.FechaCreacion);
    }
}
