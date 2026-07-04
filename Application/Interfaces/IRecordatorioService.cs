using Application.DTOs.Recordatorio;

namespace Application.Interfaces
{
    public interface IRecordatorioService
    {
        Task<Guid> CrearAsync(Guid tenantId, CrearRecordatorioDto dto, CancellationToken ct = default);
        Task<RecordatorioDto?> ObtenerPorIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
        Task<IEnumerable<RecordatorioDto>> ObtenerTodosAsync(Guid tenantId, CancellationToken ct = default);
        Task<IEnumerable<RecordatorioDto>> ObtenerPendientesDeEnvioAsync(Guid tenantId, CancellationToken ct = default);
        Task ActualizarAsync(Guid tenantId, Guid id, ActualizarRecordatorioDto dto, CancellationToken ct = default);
        Task EliminarAsync(Guid tenantId, Guid id, CancellationToken ct = default);
        Task ProcesarRecordatorioAsync(Guid recordatorioId);
        Task ProcesarPendientesAsync(CancellationToken ct = default);
    }
}
