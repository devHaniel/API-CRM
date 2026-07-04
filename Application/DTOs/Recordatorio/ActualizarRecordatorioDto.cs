namespace Application.DTOs.Recordatorio
{
    public record ActualizarRecordatorioDto(
        Guid EventoId,
        Guid? PlantillaId,
        string CanalEnvio,
        DateTime FechaProgramada,
        DateTime? FechaEnvio,
        string Estado,
        string? DetalleError);
}
