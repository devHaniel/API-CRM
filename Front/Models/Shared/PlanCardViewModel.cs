using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Front.Models.Shared
{
    public class PlanCardViewModel
    {
        public string Nombre { get; set; } = string.Empty;

        public decimal PrecioMensual { get; set; }

        public int LimiteRecordatoriosMes { get; set; }

        public int RecordatoriosUsados { get; set; }

        public int RecordatoriosRestantes =>
            Math.Max(0, LimiteRecordatoriosMes - RecordatoriosUsados);

        public int PorcentajeUso =>
            LimiteRecordatoriosMes == 0
                ? 0
                : Math.Min(100,
                    (int)Math.Round(
                        (double)RecordatoriosUsados /
                        LimiteRecordatoriosMes * 100));
    }
}