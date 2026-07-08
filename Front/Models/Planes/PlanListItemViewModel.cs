using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Front.Models.Planes
{
     public class PlanListItemViewModel
    {
        public Guid Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public decimal PrecioMensual { get; set; }

        public int LimiteRecordatoriosMes { get; set; }

        public decimal PrecioRecordatorioExtra { get; set; }

        public int MaxUsuarios { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}