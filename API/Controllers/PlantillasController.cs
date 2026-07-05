using Application.DTOs.Common;
using Application.DTOs.PlantillasMensaje;
using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlantillasController : ControllerBase
    {
        private readonly IPlantillasMensajeService _plantillasService;
        private readonly ICurrentTenantService _tenantService;

        public PlantillasController(IPlantillasMensajeService plantillasService, ICurrentTenantService tenantService)
        {
            _plantillasService = plantillasService;
            _tenantService = tenantService;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Crear(CrearPlantillasMensajeDto dto, CancellationToken ct)
        {
            var id = await _plantillasService.CrearAsync(_tenantService.TenantId, dto, ct);
            return CreatedAtAction(nameof(ObtenerPorId), new { id }, id);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlantillasMensajeDto>> ObtenerPorId(Guid id, CancellationToken ct)
        {
            var plantilla = await _plantillasService.ObtenerPorIdAsync(_tenantService.TenantId, id, ct);
            return plantilla is null ? NotFound() : Ok(plantilla);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<PlantillasMensajeDto>>> ObtenerTodos(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var plantillas = await _plantillasService.ObtenerTodosAsync(_tenantService.TenantId, pageNumber, pageSize, ct);
            return Ok(plantillas);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(Guid id, ActualizarPlantillasMensajeDto dto, CancellationToken ct)
        {
            try
            {
                await _plantillasService.ActualizarAsync(_tenantService.TenantId, id, dto, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
        {
            try
            {
                await _plantillasService.EliminarAsync(_tenantService.TenantId, id, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
