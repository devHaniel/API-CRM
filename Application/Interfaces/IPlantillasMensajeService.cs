using Application.DTOs.PlantillasMensaje;

namespace Application.Interfaces
{
    public interface IPlantillasMensajeService
    {
        Task<Guid> CrearAsync(Guid tenantId, CrearPlantillasMensajeDto dto, CancellationToken ct = default);
        Task<PlantillasMensajeDto?> ObtenerPorIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
        Task<IEnumerable<PlantillasMensajeDto>> ObtenerTodosAsync(Guid tenantId, CancellationToken ct = default);
        Task ActualizarAsync(Guid tenantId, Guid id, ActualizarPlantillasMensajeDto dto, CancellationToken ct = default);
        Task EliminarAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    }
}
