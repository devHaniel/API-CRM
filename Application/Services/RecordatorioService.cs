using Application.DTOs.Common;
using Application.DTOs.Recordatorio;
using Application.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class RecordatorioService : IRecordatorioService
    {
        private readonly IRecordatorioRepository _recordatorioRepository;
        private readonly IPlanService _planService;
        private readonly IEventoRepository _eventoRepository;
        private readonly IWhatsAppService _mensajeService;
        private readonly ILogger<RecordatorioService> _logger;

        public RecordatorioService(
            IRecordatorioRepository recordatorioRepository,
            IEventoRepository eventoRepository,
            IWhatsAppService mensajeService,
            ILogger<RecordatorioService> logger,
            IPlanService planService)
        {
            _recordatorioRepository = recordatorioRepository;
            _eventoRepository = eventoRepository;
            _mensajeService = mensajeService;
            _logger = logger;
            _planService = planService;
        }

        public async Task<Guid> CrearAsync(Guid tenantId, CrearRecordatorioDto dto, CancellationToken ct = default)
        {
            _logger.LogInformation("Iniciando creación de recordatorio para el Tenant {TenantId}. Evento: {EventoId}.", tenantId, dto.EventoId);

            if(!await _planService.PuedeEnviarMasRecordatoriosAsync(tenantId, ct))
            {
                _logger.LogWarning("El Tenant {TenantId} ha alcanzado el límite de recordatorios permitidos por su plan.", tenantId);
                throw new InvalidOperationException("El tenant ha alcanzado el límite de recordatorios permitidos por su plan.");
            }
            try
            {
                await ValidarEventoDelTenantAsync(tenantId, dto.EventoId, ct);
                await ValidarPlantillaDelTenantAsync(tenantId, dto.PlantillaId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Validación fallida al intentar crear recordatorio para el Tenant {TenantId}.", tenantId);
                throw;
            }

            var fechaProgramadaUtc = NormalizarFechaProgramada(dto.FechaProgramada);

            var recordatorio = new Recordatorio
            {
                Id = Guid.NewGuid(),
                EventoId = dto.EventoId,
                PlantillaId = dto.PlantillaId,
                CanalEnvio = dto.CanalEnvio,
                FechaProgramada = fechaProgramadaUtc,
                Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Pendiente" : dto.Estado,
                FechaCreacion = DateTime.UtcNow
            };

            await _recordatorioRepository.AddAsync(recordatorio, ct);
            await _recordatorioRepository.SaveChangesAsync(ct);

            _logger.LogInformation("Recordatorio creado exitosamente con ID {RecordatorioId} para el Tenant {TenantId}.", recordatorio.Id, tenantId);
            return recordatorio.Id;
        }

        public async Task<RecordatorioDto?> ObtenerPorIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation("Obteniendo recordatorio {RecordatorioId} para el Tenant {TenantId}.", id, tenantId);
            var recordatorio = await _recordatorioRepository.GetByIdAsync(id, tenantId, ct);
            return recordatorio is null ? null : MapToDto(recordatorio);
        }

        public async Task<PagedResultDto<RecordatorioDto>> ObtenerTodosAsync(Guid tenantId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            _logger.LogInformation("Consultando lista paginada de recordatorios para el Tenant {TenantId}. Página: {PageNumber}, Tamaño: {PageSize}", tenantId, pageNumber, pageSize);

            var totalCount = await _recordatorioRepository.CountByTenantAsync(tenantId, ct);
            var recordatorios = await _recordatorioRepository.GetPagedByTenantAsync(tenantId, pageNumber, pageSize, ct);

            var items = recordatorios.Select(MapToDto);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PagedResultDto<RecordatorioDto>(items, pageNumber, pageSize, totalCount, totalPages);
        }

        public async Task<IEnumerable<RecordatorioDto>> ObtenerPendientesDeEnvioAsync(Guid tenantId, CancellationToken ct = default)
        {
            var recordatorios = await _recordatorioRepository.GetPendientesDeEnvioAsync(tenantId, ct);
            return recordatorios.Select(MapToDto);
        }

        public async Task ActualizarAsync(Guid tenantId, Guid id, ActualizarRecordatorioDto dto, CancellationToken ct = default)
        {
            var recordatorio = await _recordatorioRepository.GetByIdAsync(id, tenantId, ct)
                ?? throw new KeyNotFoundException("Recordatorio no encontrado.");

            await ValidarEventoDelTenantAsync(tenantId, dto.EventoId, ct);
            await ValidarPlantillaDelTenantAsync(tenantId, dto.PlantillaId, ct);

            recordatorio.EventoId = dto.EventoId;
            recordatorio.PlantillaId = dto.PlantillaId;
            recordatorio.CanalEnvio = dto.CanalEnvio;
            recordatorio.FechaProgramada = NormalizarFechaProgramada(dto.FechaProgramada);
            recordatorio.FechaEnvio = dto.FechaEnvio;
            recordatorio.Estado = dto.Estado;
            recordatorio.DetalleError = dto.DetalleError;

            _recordatorioRepository.Update(recordatorio);
            await _recordatorioRepository.SaveChangesAsync(ct);
        }

        public async Task EliminarAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            var recordatorio = await _recordatorioRepository.GetByIdAsync(id, tenantId, ct)
                ?? throw new KeyNotFoundException("Recordatorio no encontrado.");

            _recordatorioRepository.Remove(recordatorio);
            await _recordatorioRepository.SaveChangesAsync(ct);
        }

        private async Task ValidarEventoDelTenantAsync(Guid tenantId, Guid eventoId, CancellationToken ct)
        {
            var evento = await _eventoRepository.GetByIdAsync(eventoId, tenantId, ct);
            if (evento is null)
                throw new InvalidOperationException("El evento no existe o no pertenece al tenant actual.");
        }

        private async Task ValidarPlantillaDelTenantAsync(Guid tenantId, Guid? plantillaId, CancellationToken ct)
        {
            if (plantillaId is null)
                return;

            var perteneceAlTenant = await _recordatorioRepository.PlantillaPerteneceAlTenantAsync(tenantId, plantillaId.Value, ct);
            if (!perteneceAlTenant)
                throw new InvalidOperationException("La plantilla no existe o no pertenece al tenant actual.");
        }

        public async Task ProcesarRecordatorioAsync(Guid recordatorioId)
        {
            var recordatorio = await _recordatorioRepository.GetByIdAsync(recordatorioId, CancellationToken.None);

            if (recordatorio is null)
                return;

            await ProcesarRecordatorioInternoAsync(recordatorio, CancellationToken.None);
            await _recordatorioRepository.SaveChangesAsync(CancellationToken.None);
        }

        public async Task ProcesarPendientesAsync(CancellationToken ct = default)
        {
            var pendientes = await _recordatorioRepository.GetPendientesDeEnvioAsync(ct);

            foreach (var recordatorio in pendientes)
            {
                await ProcesarRecordatorioInternoAsync(recordatorio, ct);
            }

            await _recordatorioRepository.SaveChangesAsync(ct);
        }

        private async Task ProcesarRecordatorioInternoAsync(Recordatorio recordatorio, CancellationToken ct)
        {
            try
            {
                if (recordatorio.Evento?.Cliente is null)
                    throw new InvalidOperationException("El recordatorio no tiene un cliente asociado.");

                if (string.IsNullOrWhiteSpace(recordatorio.Evento.Cliente.Telefono))
                    throw new InvalidOperationException("El cliente no tiene un teléfono registrado.");

                if (!string.Equals(recordatorio.CanalEnvio, "whatsapp", StringComparison.OrdinalIgnoreCase))
                    throw new NotSupportedException($"El canal de envío '{recordatorio.CanalEnvio}' no está soportado.");

                if (recordatorio.FechaProgramada > DateTime.UtcNow)
                {
                    _logger.LogInformation("Recordatorio {RecordatorioId} aún no es elegible para envío. Programado para {FechaProgramada}.", recordatorio.Id, recordatorio.FechaProgramada);
                    return;
                }

                var mensaje = ConstruirMensaje(recordatorio);
                var enviado = await _mensajeService.EnviarWhatsAppAsync(recordatorio.Evento.Cliente.Telefono, mensaje, ct);

                recordatorio.Estado = enviado ? "Enviado" : "Fallido";
                recordatorio.DetalleError = enviado ? null : "El proveedor de mensajería rechazó el envío.";
                recordatorio.FechaEnvio = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el recordatorio {RecordatorioId}.", recordatorio.Id);
                recordatorio.Estado = "Fallido";
                recordatorio.DetalleError = ex.Message;
                recordatorio.FechaEnvio = DateTime.UtcNow;
            }
            finally
            {
                _recordatorioRepository.Update(recordatorio);
            }
        }

        private static DateTime NormalizarFechaProgramada(DateTime fecha)
        {
            return fecha.Kind switch
            {
                DateTimeKind.Utc => fecha,
                DateTimeKind.Local => fecha.ToUniversalTime(),
                _ => DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
            };
        }

        private static string ConstruirMensaje(Recordatorio recordatorio)
        {
            var plantilla = recordatorio.Plantilla?.Contenido;
            if (!string.IsNullOrWhiteSpace(plantilla))
            {
                return plantilla
                    .Replace("{clienteNombre}", recordatorio.Evento?.Cliente?.Nombre ?? "cliente", StringComparison.OrdinalIgnoreCase)
                    .Replace("{fecha}", recordatorio.Evento?.Fecha.ToString("dd/MM/yyyy") ?? "la fecha", StringComparison.OrdinalIgnoreCase)
                    .Replace("{tipo}", recordatorio.Evento?.Tipo ?? "evento", StringComparison.OrdinalIgnoreCase)
                    .Replace("{descripcion}", recordatorio.Evento?.Descripcion ?? "evento", StringComparison.OrdinalIgnoreCase);
            }

            var nombre = recordatorio.Evento?.Cliente?.Nombre ?? "cliente";
            var fecha = recordatorio.Evento?.Fecha.ToString("dd/MM/yyyy") ?? "la fecha";
            return $"Hola {nombre}, te recordamos tu evento para el {fecha}.";
        }

        private static RecordatorioDto MapToDto(Recordatorio r)
            => new(
                r.Id,
                r.EventoId,
                r.Evento?.Tipo,
                r.Evento?.ClienteId ?? Guid.Empty,
                r.Evento?.Cliente?.Nombre,
                r.PlantillaId,
                r.CanalEnvio,
                r.FechaProgramada,
                r.FechaEnvio,
                r.Estado,
                r.DetalleError,
                r.FechaCreacion);
    }
}
