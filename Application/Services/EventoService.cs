using System.ComponentModel.DataAnnotations;
using Application.DTOs.Evento;
using Application.Interfaces;
using Domain;
using Domain.Interfaces;

namespace Application.Services
{
    public class EventoService : IEventoService
    {
        private readonly IEventoRepository _eventoRepository;
        private readonly IClienteRepository _clienteRepository;

        public EventoService(IEventoRepository eventoRepository, IClienteRepository clienteRepository)
        {
            _eventoRepository = eventoRepository;
            _clienteRepository = clienteRepository;
        }

        public async Task<Guid> CrearAsync(Guid tenantId, CrearEventoDto dto, CancellationToken ct = default)
        {
            await ValidarClienteDelTenantAsync(tenantId, dto.ClienteId, ct);

            // Tipo solo puede tener 2 valores:
            // Cita -> Una cita agendada : Monto puede ser null
            // Pago -> Cuota o cobro pendiente : Monto es obligatorio


            if (dto.Tipo == "Pago" && dto.Monto is null)
            {
                throw new ValidationException("El monto es obligatorio cuando el tipo es 'Pago'.");
            }

            if (dto.Tipo != "Pago" && dto.Tipo != "Cita")
            {
                throw new ValidationException($"Tipo de evento inválido: '{dto.Tipo}'.");
            }

            var evento = new Evento
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ClienteId = dto.ClienteId,
                Tipo = dto.Tipo,
                Fecha = dto.Fecha,
                Monto = dto.Monto,
                Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Pendiente" : dto.Estado, // Pendiente, Confirmado, Completado, Cancelado, Pagado, Vencido
                FechaCreacion = DateTime.UtcNow
            };


            await _eventoRepository.AddAsync(evento, ct);
            await _eventoRepository.SaveChangesAsync(ct);

            return evento.Id;
        }

        public async Task<EventoDto?> ObtenerPorIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            var evento = await _eventoRepository.GetByIdAsync(id, tenantId, ct);
            return evento is null ? null : MapToDto(evento);
        }

        public async Task<IEnumerable<EventoDto>> ObtenerTodosAsync(Guid tenantId, CancellationToken ct = default)
        {
            var eventos = await _eventoRepository.GetAllByTenantAsync(tenantId, ct);
            return eventos.Select(MapToDto);
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
            evento.Fecha = dto.Fecha;
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
                e.Monto,
                e.Estado,
                e.FechaCreacion);
    }
}
