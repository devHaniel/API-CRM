using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front.Models.Recordatorios
{
     public class RecordatorioFormViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Selecciona un evento")]
        [Display(Name = "Evento")]
        public Guid EventoId { get; set; }

        [Required(ErrorMessage = "Selecciona una plantilla")]
        [Display(Name = "Plantilla de mensaje")]
        public Guid PlantillaId { get; set; }

        [Required(ErrorMessage = "El canal es obligatorio")]
        public string CanalEnvio { get; set; } = "WhatsApp";

        [Required(ErrorMessage = "La fecha programada es obligatoria")]
        [Display(Name = "Fecha y hora de envío")]
        public DateTime FechaProgramada { get; set; } = DateTime.Now;

        public List<SelectListItem> EventosDisponibles { get; set; } = [];
        public List<SelectListItem> PlantillasDisponibles { get; set; } = [];

        public List<EventoPreviewData> EventosData { get; set; } = [];
        public List<PlantillaPreviewData> PlantillasData { get; set; } = [];
    }

    public class EventoPreviewData
    {
        public Guid Id { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class PlantillaPreviewData
    {
        public Guid Id { get; set; }
        public string Contenido { get; set; } = string.Empty;
    }
}