using Application.DTOs.Recordatorio;
using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecordatoriosController : ControllerBase
    {
        private readonly IRecordatorioService _recordatorioService;
        private readonly ICurrentTenantService _tenantService;

        public RecordatoriosController(IRecordatorioService recordatorioService, ICurrentTenantService tenantService)
        {
            _recordatorioService = recordatorioService;
            _tenantService = tenantService;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Crear(CrearRecordatorioDto dto, CancellationToken ct)
        {
            try
            {
                var id = await _recordatorioService.CrearAsync(_tenantService.TenantId, dto, ct);
                return CreatedAtAction(nameof(ObtenerPorId), new { id }, id);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RecordatorioDto>> ObtenerPorId(Guid id, CancellationToken ct)
        {
            var recordatorio = await _recordatorioService.ObtenerPorIdAsync(_tenantService.TenantId, id, ct);
            return recordatorio is null ? NotFound() : Ok(recordatorio);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecordatorioDto>>> ObtenerTodos(CancellationToken ct)
        {
            var recordatorios = await _recordatorioService.ObtenerTodosAsync(_tenantService.TenantId, ct);
            return Ok(recordatorios);
        }

        [HttpGet("pendientes-envio")]
        public async Task<ActionResult<IEnumerable<RecordatorioDto>>> ObtenerPendientesDeEnvio(CancellationToken ct)
        {
            var recordatorios = await _recordatorioService.ObtenerPendientesDeEnvioAsync(_tenantService.TenantId, ct);
            return Ok(recordatorios);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(Guid id, ActualizarRecordatorioDto dto, CancellationToken ct)
        {
            try
            {
                await _recordatorioService.ActualizarAsync(_tenantService.TenantId, id, dto, ct);
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
                await _recordatorioService.EliminarAsync(_tenantService.TenantId, id, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
