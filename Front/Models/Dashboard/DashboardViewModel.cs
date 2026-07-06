using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Front.Models.Eventos;

namespace Front.Models.Dashboard
{
    public class DashboardViewModel
    {
        public int TotalClientes { get; set; }
    public int TotalEventos { get; set; }
    public decimal IngresosTotales { get; set; }
    public int EventosPendientes { get; set; }
    public int EventosCompletados { get; set; }
    public int RecordatoriosEnviados { get; set; }
    public int RecordatoriosPendientes { get; set; }
    public int RecordatoriosFallidos { get; set; }

    public List<RecentEventoViewModel> UltimosEventos { get; set; } = [];
    }
}