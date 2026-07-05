using Application.DTOs.Common;
using Application.DTOs.Evento;
using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventosController : ControllerBase
    {
        private readonly IEventoService _eventoService;
        private readonly ICurrentTenantService _tenantService;

        public EventosController(IEventoService eventoService, ICurrentTenantService tenantService)
        {
            _eventoService = eventoService;
            _tenantService = tenantService;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Crear(CrearEventoDto dto, CancellationToken ct)
        {
            try
            {
                var id = await _eventoService.CrearAsync(_tenantService.TenantId, dto, ct);
                return CreatedAtAction(nameof(ObtenerPorId), new { id }, id);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventoDto>> ObtenerPorId(Guid id, CancellationToken ct)
        {
            var evento = await _eventoService.ObtenerPorIdAsync(_tenantService.TenantId, id, ct);
            return evento is null ? NotFound() : Ok(evento);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<EventoDto>>> ObtenerTodos(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var eventos = await _eventoService.ObtenerTodosAsync(_tenantService.TenantId, pageNumber, pageSize, ct);
            return Ok(eventos);
        }

        [HttpGet("proximos")]
        public async Task<ActionResult<IEnumerable<EventoDto>>> ObtenerProximos([FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
        {
            if (hasta < desde)
                return BadRequest(new { message = "La fecha final no puede ser menor que la fecha inicial." });

            var eventos = await _eventoService.ObtenerProximosAsync(_tenantService.TenantId, desde, hasta, ct);
            return Ok(eventos);
        }

        [HttpGet("pagos-pendientes")]
        public async Task<ActionResult<IEnumerable<EventoDto>>> ObtenerPendientesDePago(CancellationToken ct)
        {
            var eventos = await _eventoService.ObtenerPendientesDePagoAsync(_tenantService.TenantId, ct);
            return Ok(eventos);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(Guid id, ActualizarEventoDto dto, CancellationToken ct)
        {
            try
            {
                await _eventoService.ActualizarAsync(_tenantService.TenantId, id, dto, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
        {
            try
            {
                await _eventoService.EliminarAsync(_tenantService.TenantId, id, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
