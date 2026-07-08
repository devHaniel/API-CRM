using System.Security.Claims;
using Application.DTOs.Auth;
using Application.Interfaces;
using Front.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Front.Controllers
{
    [Route("[controller]/[action]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            var viewModel = new RegisterViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind(Prefix = "Input")] RegisterInputModel input, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var invalidViewModel = new RegisterViewModel { Input = input };
                return View(invalidViewModel);
            }

            try
            {
                var dto = new RegisterTenantDto(
                    input.NombreNegocio,
                    input.Rubro,
                    input.Email,
                    input.Password,
                    input.NombreAdmin
                );

                var resultado = await _authService.RegisterTenantAsync(dto, ct);

                TempData["RegistroExitoso"] = $"Cuenta creada para {resultado.NombreNegocio}. Ahora inicia sesión.";
                return RedirectToAction(nameof(Login));
            }
            catch (InvalidOperationException ex)
            {
                var errorViewModel = new RegisterViewModel
                {
                    Input = input,
                    ErrorMessage = ex.Message
                };

                return View("Register", errorViewModel);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var authResult = await _authService.LoginAsync(new LoginDto(model.Email, model.Password), ct);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, authResult.Email),
                    new(ClaimTypes.Name, authResult.Email),
                    new("tenant_id", authResult.TenantId.ToString()),
                    new("TenantId", authResult.TenantId.ToString()),
                    new("NombreNegocio", authResult.NombreNegocio),
                    new("Email", authResult.Email),
                    new(ClaimTypes.Role, authResult.Rol)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    });

                return RedirectToAction("Index", "Dashboard");
            }
            catch (UnauthorizedAccessException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}
