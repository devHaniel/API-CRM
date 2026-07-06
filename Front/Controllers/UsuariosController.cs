using Application.DTOs.Auth;
using Application.Interfaces;
using Domain;
using Domain.Interfaces;
using Front.Models.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Front.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class UsuariosController : Controller
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAuthService _authService;
        private readonly ICurrentTenantService _currentTenantService;

        public UsuariosController(IUsuarioRepository usuarioRepository, IAuthService authService, ICurrentTenantService currentTenantService)
        {
            _usuarioRepository = usuarioRepository;
            _authService = authService;
            _currentTenantService = currentTenantService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;
            var usuarios = (await _usuarioRepository.GetAllAsync(tenantId, ct))
                .ToList();
            var model = usuarios.Select(u => new UsuarioListItemViewModel
            {
                Id = u.Id,
                Email = u.Email,
                Rol = u.Rol,
                Activo = u.Activo,
                FechaCreacion = u.FechaCreacion
            }).ToList();

            return View(model);
        }

        public IActionResult Create()
        {
            return View(new UsuarioFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "La contraseña es obligatoria.");
                return View(model);
            }

            var tenantId = _currentTenantService.TenantId;
            var existe = (await _usuarioRepository.GetAllAsync(tenantId, ct))
                .Any(u => u.TenantId == tenantId && u.Email == model.Email);
            if (existe)
            {
                ModelState.AddModelError(nameof(model.Email), "Ya existe un usuario con ese email.");
                return View(model);
            }

            var usuario = new CrearUsuarioDto(
                model.Email,
                model.Password,
                model.Rol
            );

            await _authService.CrearUsuarioAsync(usuario, ct);

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;
            var usuario = (await _usuarioRepository.GetAllAsync(tenantId, ct))
                .FirstOrDefault(u => u.Id == id && u.TenantId == tenantId);
            if (usuario is null)
                return NotFound();

            var model = new UsuarioFormViewModel
            {
                Id = usuario.Id,
                Email = usuario.Email,
                Rol = usuario.Rol,
                Activo = usuario.Activo
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsuarioFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tenantId = _currentTenantService.TenantId;
            var usuario = (await _usuarioRepository.GetAllAsync(tenantId, ct))
                .FirstOrDefault(u => u.Id == model.Id && u.TenantId == tenantId);
            if (usuario is null)
                return NotFound();

            usuario.Email = model.Email;
            usuario.Rol = model.Rol;
            usuario.Activo = model.Activo;
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                usuario.PasswordHash = model.Password;
            }

            _usuarioRepository.Update(usuario);
            await _usuarioRepository.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;
            var usuario = (await _usuarioRepository.GetAllAsync(tenantId, ct))
                .FirstOrDefault(u => u.Id == id && u.TenantId == tenantId);
            if (usuario is null)
                return NotFound();

            _usuarioRepository.Remove(usuario);
            await _usuarioRepository.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Index));
        }
    }
}
