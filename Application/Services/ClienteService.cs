using Application.DTOs.Cliente;
using Application.DTOs.Common;
using Application.DTOs.Evento;
using Application.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(IClienteRepository clienteRepository, ILogger<ClienteService> _logger)
        {
            _clienteRepository = clienteRepository;
            this._logger = _logger;
        }

        public async Task<Guid> CrearAsync(Guid tenantId, CrearClienteDto dto, CancellationToken ct = default)
        {
            _logger.LogInformation("Iniciando creación de cliente para el Tenant {TenantId}. Nombre: {Nombre}", tenantId, dto.Nombre);

            if (await _clienteRepository.ExisteTelefonoAsync(tenantId, dto.Telefono, ct))
            {
                _logger.LogWarning("No se pudo crear el cliente. El teléfono {Telefono} ya existe en el Tenant {TenantId}.", dto.Telefono, tenantId);
                throw new InvalidOperationException("El teléfono ya está registrado.");
            }
            if (await _clienteRepository.ExisteEmailAsync(tenantId, dto.Email, ct))
            {
                _logger.LogWarning("No se pudo crear el cliente. El email {Email} ya existe en el Tenant {TenantId}.", dto.Email, tenantId);
                throw new InvalidOperationException("El email ya está registrado.");
            }

            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Notas = dto.Notas,
                Email = dto.Email,
                FechaCreacion = DateTime.UtcNow,
                Activo = true
            };

            await _clienteRepository.AddAsync(cliente, ct);
            await _clienteRepository.SaveChangesAsync(ct);

            _logger.LogInformation("Cliente creado exitosamente con ID {ClienteId} para el Tenant {TenantId}.", cliente.Id, tenantId);
            return cliente.Id;
        }

        public async Task<ClienteDto?> ObtenerPorIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation("Obteniendo cliente {ClienteId} para el Tenant {TenantId}.", id, tenantId);
            var cliente = await _clienteRepository.GetByIdAsync(id, tenantId, ct);
            return cliente is null ? null : MapToDto(cliente);
        }

        public async Task<PagedResultDto<ClienteDto>> ObtenerTodosAsync(Guid tenantId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            _logger.LogInformation("Consultando lista paginada de clientes para el Tenant {TenantId}. Página: {PageNumber}, Tamaño: {PageSize}", tenantId, pageNumber, pageSize);
            
            var totalCount = await _clienteRepository.CountByTenantAsync(tenantId, ct);
            var clientes = await _clienteRepository.GetPagedByTenantAsync(tenantId, pageNumber, pageSize, ct);
            
            var items = clientes.Select(MapToDto);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PagedResultDto<ClienteDto>(items, pageNumber, pageSize, totalCount, totalPages);
        }

        public async Task ActualizarAsync(Guid tenantId, Guid id, CrearClienteDto dto, CancellationToken ct = default)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id, tenantId, ct)
                ?? throw new KeyNotFoundException("Cliente no encontrado.");

            if (cliente.TenantId != tenantId)
                throw new UnauthorizedAccessException("No tiene permiso para actualizar este cliente.");

            if (dto.Email != cliente.Email && await _clienteRepository.ExisteEmailAsync(tenantId, dto.Email, ct))
                throw new InvalidOperationException("El email ya está registrado.");
            if (dto.Telefono != cliente.Telefono && await _clienteRepository.ExisteTelefonoAsync(tenantId, dto.Telefono, ct))
                throw new InvalidOperationException("El teléfono ya está registrado.");

            cliente.Nombre = dto.Nombre;
            cliente.Telefono = dto.Telefono;
            cliente.Email = dto.Email;
            cliente.Notas = dto.Notas;
            _clienteRepository.Update(cliente);
            await _clienteRepository.SaveChangesAsync(ct);
        }

        public async Task EliminarAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id, tenantId, ct)
                ?? throw new KeyNotFoundException("Cliente no encontrado.");

            if (cliente.TenantId != tenantId)
                throw new UnauthorizedAccessException("No tiene permiso para actualizar este cliente.");

            _clienteRepository.Remove(cliente);
            await _clienteRepository.SaveChangesAsync(ct);
        }

        private static ClienteDto MapToDto(Cliente c)
            => new(c.Id, c.Nombre, c.Telefono, c.Email, c.Notas, c.FechaCreacion, c.Activo);

    }
}
