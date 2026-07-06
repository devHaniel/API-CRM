using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Front.Models.Clientes
{
    public class CllienteListItemViewModel
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public bool Activo { get; set; }
    }
}