namespace Application.DTOs.Evento
{
    public record EventoDto(
        Guid Id,
        Guid ClienteId,
        string? ClienteNombre,
        string Tipo,
        DateTime Fecha,
        string? Descripcion,
        decimal? Monto,
        string Estado,
        DateTime FechaCreacion);
}
