using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Front.Models.Eventos
{
    public class RecentEventoViewModel
{
    public Guid Id { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}
}