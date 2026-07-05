using System;
using System.Collections.Generic;

namespace Application.DTOs.Dashboard
{
    public record TenantDashboardDto(
        int TotalClientes,
        int TotalEventos,
        decimal IngresosTotales,
        int EventosPendientes,
        int EventosCompletados,
        int RecordatoriosEnviados,
        int RecordatoriosPendientes,
        int RecordatoriosFallidos,
        List<RecentEventoDto> UltimosEventos
    );

    public record RecentEventoDto(
        Guid EventoId,
        string ClienteNombre,
        DateTime Fecha,
        decimal Monto,
        string Estado,
        string Tipo
    );
}
