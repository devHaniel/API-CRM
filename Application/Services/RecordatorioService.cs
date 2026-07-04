using Application.DTOs.Recordatorio;
using Application.Interfaces;
using Domain;
using Domain.Interfaces;

namespace Application.Services
{
    public class RecordatorioService : IRecordatorioService
    {
        private readonly IRecordatorioRepository _recordatorioRepository;
        private readonly IEventoRepository _eventoRepository;
        private readonly IMensajeService _mensajeService;
        public RecordatorioService(IRecordatorioRepository recordatorioRepository, IEventoRepository eventoRepository, IMensajeService mensajeService)
        {
            _recordatorioRepository = recordatorioRepository;
            _eventoRepository = eventoRepository;
            _mensajeService = mensajeService;
        }

        public async Task<Guid> CrearAsync(Guid tenantId, CrearRecordatorioDto dto, CancellationToken ct = default)
        {
            await ValidarEventoDelTenantAsync(tenantId, dto.EventoId, ct);
            await ValidarPlantillaDelTenantAsync(tenantId, dto.PlantillaId, ct);

            var recordatorio = new Recordatorio
            {
                Id = Guid.NewGuid(),
                EventoId = dto.EventoId,
                PlantillaId = dto.PlantillaId,
                CanalEnvio = dto.CanalEnvio,
                FechaProgramada = dto.FechaProgramada,
                Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Pendiente" : dto.Estado,
                FechaCreacion = DateTime.UtcNow
            };

            await _recordatorioRepository.AddAsync(recordatorio, ct);
            await _recordatorioRepository.SaveChangesAsync(ct);

            return recordatorio.Id;
        }

        public async Task<RecordatorioDto?> ObtenerPorIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            var recordatorio = await _recordatorioRepository.GetByIdAsync(id, tenantId, ct);
            return recordatorio is null ? null : MapToDto(recordatorio);
        }

        public async Task<IEnumerable<RecordatorioDto>> ObtenerTodosAsync(Guid tenantId, CancellationToken ct = default)
        {
            var recordatorios = await _recordatorioRepository.GetAllByTenantAsync(tenantId, ct);
            return recordatorios.Select(MapToDto);
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
            recordatorio.FechaProgramada = dto.FechaProgramada;
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

        public async Task ProcesarPendientesAsync(CancellationToken ct = default)
        {
            var pendientes = await _recordatorioRepository.GetPendientesDeEnvioAsync(ct);

            foreach (var recordatorio in pendientes)
            {
                try
                {
                    if (recordatorio.Evento?.Cliente is null)
                        throw new InvalidOperationException("El recordatorio no tiene un cliente asociado.");

                    if (string.IsNullOrWhiteSpace(recordatorio.Evento.Cliente.Telefono))
                        throw new InvalidOperationException("El cliente no tiene un teléfono registrado.");

                    if (!string.Equals(recordatorio.CanalEnvio, "whatsapp", StringComparison.OrdinalIgnoreCase))
                        throw new NotSupportedException($"El canal de envío '{recordatorio.CanalEnvio}' no está soportado.");

                    var mensaje = ConstruirMensaje(recordatorio);
                    var enviado = await _mensajeService.EnviarWhatsAppAsync(recordatorio.Evento.Cliente.Telefono, mensaje, ct);

                    recordatorio.Estado = enviado ? "Enviado" : "Fallido";
                    recordatorio.DetalleError = enviado ? null : "El proveedor de mensajería rechazó el envío.";
                    recordatorio.FechaEnvio = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    recordatorio.Estado = "Fallido";
                    recordatorio.DetalleError = ex.Message;
                    recordatorio.FechaEnvio = DateTime.UtcNow;
                }
                finally
                {
                    _recordatorioRepository.Update(recordatorio);
                }
            }

            await _recordatorioRepository.SaveChangesAsync(ct);
        }

        private static string ConstruirMensaje(Recordatorio recordatorio)
        {
            var plantilla = recordatorio.Plantilla?.Contenido;
            if (!string.IsNullOrWhiteSpace(plantilla))
            {
                return plantilla
                    .Replace("{clienteNombre}", recordatorio.Evento?.Cliente?.Nombre ?? "cliente", StringComparison.OrdinalIgnoreCase)
                    .Replace("{fecha}", recordatorio.Evento?.Fecha.ToString("dd/MM/yyyy") ?? "la fecha", StringComparison.OrdinalIgnoreCase)
                    .Replace("{tipo}", recordatorio.Evento?.Tipo ?? "evento", StringComparison.OrdinalIgnoreCase);
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
