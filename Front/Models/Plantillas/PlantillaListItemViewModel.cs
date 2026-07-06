using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Front.Models.Plantillas
{
    public class PlantillaListItemViewModel
    {
        public Guid Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}