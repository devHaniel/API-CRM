using Application.DTOs.Recordatorio;

namespace Front.Models.Eventos
{
    public class EventoDetailsViewModel
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Monto { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public List<RecordatorioDto> Recordatorios { get; set; } = [];
    }
}
