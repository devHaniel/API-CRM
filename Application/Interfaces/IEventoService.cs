using Application.DTOs.Evento;

namespace Application.Interfaces
{
    public interface IEventoService
    {
        Task<Guid> CrearAsync(Guid tenantId, CrearEventoDto dto, CancellationToken ct = default);
        Task<EventoDto?> ObtenerPorIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
        Task<IEnumerable<EventoDto>> ObtenerTodosAsync(Guid tenantId, CancellationToken ct = default);
        Task<IEnumerable<EventoDto>> ObtenerProximosAsync(Guid tenantId, DateTime desde, DateTime hasta, CancellationToken ct = default);
        Task<IEnumerable<EventoDto>> ObtenerPendientesDePagoAsync(Guid tenantId, CancellationToken ct = default);
        Task ActualizarAsync(Guid tenantId, Guid id, ActualizarEventoDto dto, CancellationToken ct = default);
        Task EliminarAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    }
}
