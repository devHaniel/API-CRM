namespace Application.DTOs.Recordatorio
{
    public record RecordatorioDto(
        Guid Id,
        Guid EventoId,
        string? EventoTipo,
        Guid ClienteId,
        string? ClienteNombre,
        Guid? PlantillaId,
        string CanalEnvio,
        DateTime FechaProgramada,
        DateTime? FechaEnvio,
        string Estado,
        string? DetalleError,
        DateTime FechaCreacion);
}
