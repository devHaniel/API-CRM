using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register-tenant")]
    public async Task<ActionResult<AuthResponseDto>> RegisterTenant(RegisterTenantDto dto, CancellationToken ct)
    {
        try
        {
            var response = await _authService.RegisterTenantAsync(dto, ct);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto, CancellationToken ct)
    {
        try
        {
            var response = await _authService.LoginAsync(dto, ct);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("usuarios")]
    [Authorize(Roles = "Admin")]   // <-- SOLO un Admin autenticado puede llegar aquí
    public async Task<ActionResult<Guid>> CrearUsuario(CrearUsuarioDto dto, CancellationToken ct)
    {
        try
        {
            var id = await _authService.CrearUsuarioAsync(dto, ct);
            return Ok(id);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
}