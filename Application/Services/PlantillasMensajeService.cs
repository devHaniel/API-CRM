using Application.DTOs.PlantillasMensaje;
using Application.Interfaces;
using Domain;
using Domain.Interfaces;

namespace Application.Services
{
    public class PlantillasMensajeService : IPlantillasMensajeService
    {
        private readonly IPlantillasMensajeRepository _plantillasMensajeRepository;

        public PlantillasMensajeService(IPlantillasMensajeRepository plantillasMensajeRepository)
        {
            _plantillasMensajeRepository = plantillasMensajeRepository;
        }

        public async Task<Guid> CrearAsync(Guid tenantId, CrearPlantillasMensajeDto dto, CancellationToken ct = default)
        {
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

            return plantilla.Id;
        }

        public async Task<PlantillasMensajeDto?> ObtenerPorIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            var plantilla = await _plantillasMensajeRepository.GetByIdAsync(id, tenantId, ct);
            return plantilla is null ? null : MapToDto(plantilla);
        }

        public async Task<IEnumerable<PlantillasMensajeDto>> ObtenerTodosAsync(Guid tenantId, CancellationToken ct = default)
        {
            var plantillas = await _plantillasMensajeRepository.GetAllByTenantAsync(tenantId, ct);
            return plantillas.Select(MapToDto);
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
