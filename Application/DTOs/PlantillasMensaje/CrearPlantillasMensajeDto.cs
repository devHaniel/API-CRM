namespace Application.DTOs.PlantillasMensaje
{
    public record CrearPlantillasMensajeDto(
        string Tipo,
        string Contenido,
        bool? Activo);
}
