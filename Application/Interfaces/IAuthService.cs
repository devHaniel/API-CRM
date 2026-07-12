using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Auth;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterTenantAsync(RegisterTenantDto dto, CancellationToken ct = default);
        Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
        Task<Guid> CrearUsuarioAsync(CrearUsuarioDto dto, CancellationToken ct = default);
        Task<string> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct = default);
        Task ResetPasswordAsync(string email, string newPassword, CancellationToken ct = default);
    }
}