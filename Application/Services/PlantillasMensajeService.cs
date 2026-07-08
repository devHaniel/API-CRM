using Application.DTOs.Common;
using Application.DTOs.PlantillasMensaje;
using Application.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class PlantillasMensajeService : IPlantillasMensajeService
    {
        private readonly IPlantillasMensajeRepository _plantillasMensajeRepository;
        private readonly ILogger<PlantillasMensajeService> _logger;

        public PlantillasMensajeService(IPlantillasMensajeRepository plantillasMensajeRepository, ILogger<PlantillasMensajeService> logger)
        {
            _plantillasMensajeRepository = plantillasMensajeRepository;
            _logger = logger;
        }

        public async Task<Guid> CrearAsync(Guid tenantId, CrearPlantillasMensajeDto dto, CancellationToken ct = default)
        {
            _logger.LogInformation("Creando nueva plantilla de mensaje tipo {Tipo} para el Tenant {TenantId}.", dto.Tipo, tenantId);

            var plantilla = new PlantillasMensaje
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Tipo = dto.Tipo,
                Contenido = dto.Contenido,
                Activo = dto.Activo ?? true,
                FechaCreacion = DateTime.UtcNow
            };

            await _plantillasMensajeRepository.AddAsync(plantilla, ct);
            await _plantillasMensajeRepository.SaveChangesAsync(ct);

            _logger.LogInformation("Plantilla creada exitosamente con ID {PlantillaId} para el Tenant {TenantId}.", plantilla.Id, tenantId);
            return plantilla.Id;
        }

        public async Task<PlantillasMensajeDto?> ObtenerPorIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation("Obteniendo plantilla {PlantillaId} para el Tenant {TenantId}.", id, tenantId);
            var plantilla = await _plantillasMensajeRepository.GetByIdAsync(id, tenantId, ct);
            return plantilla is null ? null : MapToDto(plantilla);
        }

        public async Task<PagedResultDto<PlantillasMensajeDto>> ObtenerTodosAsync(Guid tenantId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            _logger.LogInformation("Consultando lista paginada de plantillas para el Tenant {TenantId}. Página: {PageNumber}, Tamaño: {PageSize}", tenantId, pageNumber, pageSize);
            
            var totalCount = await _plantillasMensajeRepository.CountByTenantAsync(tenantId, ct);
            var plantillas = await _plantillasMensajeRepository.GetPagedByTenantAsync(tenantId, pageNumber, pageSize, ct);
            
            var items = plantillas.Select(MapToDto);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PagedResultDto<PlantillasMensajeDto>(items, pageNumber, pageSize, totalCount, totalPages);
        }

        public async Task ActualizarAsync(Guid tenantId, Guid id, ActualizarPlantillasMensajeDto dto, CancellationToken ct = default)
        {
            var plantilla = await _plantillasMensajeRepository.GetByIdAsync(id, tenantId, ct)
                ?? throw new KeyNotFoundException("Plantilla de mensaje no encontrada.");

            plantilla.Tipo = dto.Tipo;
            plantilla.Contenido = dto.Contenido;
            plantilla.Activo = dto.Activo;

            _plantillasMensajeRepository.Update(plantilla);
            await _plantillasMensajeRepository.SaveChangesAsync(ct);
        }

        public async Task EliminarAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            var plantilla = await _plantillasMensajeRepository.GetByIdAsync(id, tenantId, ct)
                ?? throw new KeyNotFoundException("Plantilla de mensaje no encontrada.");

            _plantillasMensajeRepository.Remove(plantilla);
            await _plantillasMensajeRepository.SaveChangesAsync(ct);
        }

        private static PlantillasMensajeDto MapToDto(PlantillasMensaje p)
            => new(
                p.Id,
                p.Tipo,
                p.Contenido,
                p.Activo,
                p.FechaCreacion);
    }
}
