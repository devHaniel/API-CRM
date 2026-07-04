namespace Application.DTOs.Recordatorio
{
    public record CrearRecordatorioDto(
        Guid EventoId,
        Guid? PlantillaId,
        string CanalEnvio,
        DateTime FechaProgramada,
        string? Estado);
}
