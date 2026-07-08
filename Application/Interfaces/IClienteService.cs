using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Cliente;
using Application.DTOs.Common;

namespace Application.Interfaces
{
public interface IClienteService
{
    Task<Guid> CrearAsync(Guid tenantId, CrearClienteDto dto, CancellationToken ct = default);
    Task<ClienteDto?> ObtenerPorIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<PagedResultDto<ClienteDto>> ObtenerTodosAsync(Guid tenantId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task ActualizarAsync(Guid tenantId, Guid id, CrearClienteDto dto, CancellationToken ct = default);
    Task EliminarAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}
}
