using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Application.DTOs.Auth
{
    public record RegisterTenantDto(
        [Required, MaxLength(150)] string NombreNegocio,
        [Required, MaxLength(80)] string Rubro,
        [Required, EmailAddress] string Email,
        [Required, MinLength(8)] string Password,
        [Required] string NombreAdmin
    );

    public record LoginDto(
        [Required, EmailAddress] string Email,
        [Required] string Password
    );

    public record AuthResponseDto(
        string Token,
        string Email,
        string Rol,
        Guid TenantId,
        string NombreNegocio
    );

    public record CrearUsuarioDto(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required] string Rol   // "Admin" o "Empleado"
);
}