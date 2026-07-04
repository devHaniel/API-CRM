namespace Application.DTOs.Evento
{
    public record CrearEventoDto(
        Guid ClienteId,
        string Tipo,
        DateTime Fecha,
        decimal? Monto,
        string? Estado);
}
