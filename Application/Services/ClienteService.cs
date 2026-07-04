using Application.DTOs.Cliente;
using Application.Interfaces;
using Domain;
using Domain.Interfaces;

namespace Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<Guid> CrearAsync(Guid tenantId, CrearClienteDto dto, CancellationToken ct = default)
        {
            if (await _clienteRepository.ExisteTelefonoAsync(tenantId, dto.Telefono, ct))
                throw new InvalidOperationException("El teléfono ya está registrado.");
            if (await _clienteRepository.ExisteEmailAsync(tenantId, dto.Email, ct))
                throw new InvalidOperationException("El email ya está registrado.");

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

            return cliente.Id;
        }

        public async Task<ClienteDto?> ObtenerPorIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id, tenantId, ct);
            return cliente is null ? null : MapToDto(cliente);
        }

        public async Task<IEnumerable<ClienteDto>> ObtenerTodosAsync(Guid tenantId, CancellationToken ct = default)
        {
            var clientes = await _clienteRepository.GetAllByTenantAsync(tenantId, ct);
            return clientes.Select(MapToDto);
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
