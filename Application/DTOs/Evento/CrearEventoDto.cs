namespace Application.DTOs.Evento
{
    public record CrearEventoDto(
        Guid ClienteId,
        string Tipo,
        DateTime Fecha,
        string? Descripcion,
        decimal? Monto,
        string? Estado);
}
