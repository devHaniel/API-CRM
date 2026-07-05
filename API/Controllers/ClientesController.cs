using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Cliente;
using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly ICurrentTenantService _tenantService;

        public ClientesController(IClienteService clienteService, ICurrentTenantService tenantService)
        {
            _clienteService = clienteService;
            _tenantService = tenantService;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Crear(CrearClienteDto dto, CancellationToken ct)
        {
            var id = await _clienteService.CrearAsync(_tenantService.TenantId, dto, ct);
            return CreatedAtAction(nameof(ObtenerPorId), new { id }, id);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> ObtenerPorId(Guid id, CancellationToken ct)
        {
            var cliente = await _clienteService.ObtenerPorIdAsync(_tenantService.TenantId, id, ct);
            return cliente is null ? NotFound() : Ok(cliente);
        }

        [HttpGet]
        public async Task<ActionResult<Application.DTOs.Common.PagedResultDto<ClienteDto>>> ObtenerTodos(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var clientes = await _clienteService.ObtenerTodosAsync(_tenantService.TenantId, pageNumber, pageSize, ct);
            return Ok(clientes);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(Guid id, CrearClienteDto dto, CancellationToken ct)
        {
            await _clienteService.ActualizarAsync(_tenantService.TenantId, id, dto, ct);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
        {
            await _clienteService.EliminarAsync(_tenantService.TenantId, id, ct);
            return NoContent();
        }
    }
}
