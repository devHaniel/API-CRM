using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Front.Models.Pagos
{
    public class PagoListItemViewModel
    {
        public Guid Id { get; set; }
        public string? ClienteNombre { get; set; }
        public string? Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public decimal? Monto { get; set; }
        public string Estado { get; set; } = string.Empty;

        public bool EstaVencido => Fecha < DateTime.Now && Estado != "Pagado";
    }
}