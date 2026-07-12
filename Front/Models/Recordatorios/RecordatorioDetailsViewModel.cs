namespace Front.Models.Recordatorios
{
    public class RecordatorioDetailsViewModel
    {
        public Guid Id { get; set; }
        public Guid EventoId { get; set; }
        public string? EventoTipo { get; set; }
        public Guid ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public Guid? PlantillaId { get; set; }
        public string CanalEnvio { get; set; } = string.Empty;
        public DateTime FechaProgramada { get; set; }
        public DateTime? FechaEnvio { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? DetalleError { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
