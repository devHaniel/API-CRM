using Application.DTOs.Auth;
using Application.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ICurrentTenantService _tenantService;
        private readonly IEmailService _emailService;
        private readonly IPlanService _planService;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            ITenantRepository tenantRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            ICurrentTenantService tenantService,
            IPlanService planService,
            IEmailService emailService)
        {
            _usuarioRepository = usuarioRepository;
            _tenantRepository = tenantRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _tenantService = tenantService;
            _planService = planService;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> RegisterTenantAsync(RegisterTenantDto dto, CancellationToken ct = default)
        {
            if (await _usuarioRepository.ExisteEmailAsync(dto.Email, ct))
                throw new InvalidOperationException("Ya existe una cuenta con ese email.");

            var planId = await _planService.ObtenerTodosAsync();

            if(planId.Items.Count() == 0)
                throw new InvalidOperationException("No hay planes disponibles para asignar al nuevo tenant.");

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Nombre = dto.NombreNegocio,
                Rubro = dto.Rubro,
                PlanId = planId.Items.First().Id,
                FechaCreacion = DateTime.UtcNow,
                Activo = true
            };
            await _tenantRepository.AddAsync(tenant, ct);

            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = dto.Email,
                PasswordHash = _passwordHasher.Hash(dto.Password),
                Rol = "Admin",
                FechaCreacion = DateTime.UtcNow,
                Activo = true
            };
            await _usuarioRepository.AddAsync(usuario, ct);
            await _tenantRepository.SaveChangesAsync(ct);

            var token = _jwtTokenGenerator.GenerarToken(usuario);

            await _emailService.SendWelcomeEmailAsync(
                usuario.Email,
                usuario.Email.Split('@')[0],
                ct);

            return new AuthResponseDto(token, usuario.Email, usuario.Rol, tenant.Id, tenant.Nombre);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
        {
            var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email, ct)
                ?? throw new UnauthorizedAccessException("Credenciales inválidas.");

            if (!usuario.Activo)
                throw new UnauthorizedAccessException("Usuario inactivo.");

            if (!_passwordHasher.Verificar(dto.Password, usuario.PasswordHash))
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            var tenant = await _tenantRepository.GetByIdAsync(usuario.TenantId, ct)
                ?? throw new InvalidOperationException("Tenant no encontrado.");

            var token = _jwtTokenGenerator.GenerarToken(usuario);

            return new AuthResponseDto(token, usuario.Email, usuario.Rol, tenant.Id, tenant.Nombre);
        }

        public async Task<Guid> CrearUsuarioAsync(CrearUsuarioDto dto, CancellationToken ct = default)
        {
            if (await _usuarioRepository.ExisteEmailAsync(dto.Email, ct))
                throw new InvalidOperationException("Ya existe una cuenta con ese email.");

            if (dto.Rol != "Admin" && dto.Rol != "Empleado")
                throw new ArgumentException("Rol inválido. Debe ser 'Admin' o 'Empleado'.");

            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantService.TenantId,
                Email = dto.Email,
                PasswordHash = _passwordHasher.Hash(dto.Password),
                Rol = dto.Rol,
                FechaCreacion = DateTime.UtcNow,
                Activo = true
            };

            await _usuarioRepository.AddAsync(usuario, ct);
            await _usuarioRepository.SaveChangesAsync(ct);

            return usuario.Id;
        }
    }
}
