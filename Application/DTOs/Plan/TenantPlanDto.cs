using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.DTOs.Plan
{
    public record TenantPlanDto(
        Guid PlanId,
        string Nombre,
        decimal PrecioMensual,
        int LimiteRecordatoriosMes,
        decimal PrecioRecordatorioExtra,
        int MaxUsuarios,
        int UsoActualMes
    );
}