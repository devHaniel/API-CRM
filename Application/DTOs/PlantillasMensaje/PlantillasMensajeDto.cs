namespace Application.DTOs.PlantillasMensaje
{
    public record PlantillasMensajeDto(
        Guid Id,
        string Tipo,
        string Contenido,
        bool Activo,
        DateTime FechaCreacion);
}
