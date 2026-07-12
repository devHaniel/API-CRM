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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new ForgotPasswordDto(model.Email);
            var code = await _authService.ForgotPasswordAsync(dto, ct);

            if (!string.IsNullOrEmpty(code))
            {
                Response.Cookies.Append("PasswordReset", $"{model.Email}|{code}", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(15)
                });
            }

            model.Enviado = true;
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var cookie = Request.Cookies["PasswordReset"];
            if (cookie is null)
                return RedirectToAction(nameof(ForgotPassword));

            var parts = cookie.Split('|', 2);
            if (parts.Length != 2)
                return RedirectToAction(nameof(ForgotPassword));

            return View(new ResetPasswordViewModel { Email = parts[0] });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            var cookie = Request.Cookies["PasswordReset"];
            if (cookie is null)
            {
                ModelState.AddModelError(string.Empty, "Solicita un nuevo código de verificación.");
                return View(model);
            }

            var parts = cookie.Split('|', 2);
            if (parts.Length != 2 || parts[0] != model.Email)
            {
                ModelState.AddModelError(string.Empty, "El email no coincide con la solicitud.");
                return View(model);
            }

            if (parts[1] != model.Token)
            {
                ModelState.AddModelError(string.Empty, "Código de verificación inválido.");
                return View(model);
            }

            try
            {
                await _authService.ResetPasswordAsync(model.Email, model.NewPassword, ct);

                Response.Cookies.Delete("PasswordReset");
                TempData["RegistroExitoso"] = "Contraseña restablecida correctamente. Inicia sesión.";
                return RedirectToAction(nameof(Login));
            }
            catch (InvalidOperationException ex)
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
