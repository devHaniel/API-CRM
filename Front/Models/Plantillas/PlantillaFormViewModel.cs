using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front.Models.Plantillas
{
    public class PlantillaFormViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        [Display(Name = "Tipo de plantilla")]
        public string Tipo { get; set; } = "RecordatorioCita";

        [Required(ErrorMessage = "El contenido es obligatorio")]
        [MaxLength(500)]
        [Display(Name = "Contenido del mensaje")]
        public string Contenido { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public List<SelectListItem> TiposDisponibles { get; } =
        [
            new("RecordatorioCita", "Recordatorio de Cita"),
            new("RecordatorioPago", "Recordatorio de Pago")
        ];
    }
}