namespace Application.DTOs.Evento
{
    public record ActualizarEventoDto(
        Guid ClienteId,
        string Tipo,
        DateTime Fecha,
        decimal? Monto,
        string Estado);
}
