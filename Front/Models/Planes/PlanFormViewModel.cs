using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Front.Models.Planes
{
    public class PlanFormViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Precio mensual")]
        public decimal PrecioMensual { get; set; }

        [Required]
        [Display(Name = "Límite de recordatorios por mes")]
        public int LimiteRecordatoriosMes { get; set; }

        [Required]
        [Display(Name = "Precio por recordatorio extra")]
        public decimal PrecioRecordatorioExtra { get; set; }

        [Required]
        [Display(Name = "Máximo de usuarios")]
        public int MaxUsuarios { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
    }
}