using Application.DTOs.PlantillasMensaje;

namespace Application.Interfaces
{
    public interface IPlantillasMensajeService
    {
        Task<Guid> CrearAsync(Guid tenantId, CrearPlantillasMensajeDto dto, CancellationToken ct = default);
        Task<PlantillasMensajeDto?> ObtenerPorIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
        Task<Application.DTOs.Common.PagedResultDto<PlantillasMensajeDto>> ObtenerTodosAsync(Guid tenantId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
        Task ActualizarAsync(Guid tenantId, Guid id, ActualizarPlantillasMensajeDto dto, CancellationToken ct = default);
        Task EliminarAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    }
}
