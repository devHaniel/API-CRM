using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.DTOs.Plan
{
    public record PlanDto(
        Guid Id,
        string Nombre,
        decimal PrecioMensual,
        int LimiteRecordatoriosMes,
        decimal PrecioRecordatorioExtra,
        int MaxUsuarios,
        bool Activo,
        DateTime FechaCreacion
    );
}