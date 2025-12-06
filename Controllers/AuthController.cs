using GestionLaboresAcademicas.Data;
using GestionLaboresAcademicas.Helpers;
using GestionLaboresAcademicas.Models;
using GestionLaboresAcademicas.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GestionLaboresAcademicas.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.CredencialAcceso)
                .FirstOrDefaultAsync(u =>
                    (u.CredencialAcceso != null && u.CredencialAcceso.Username == model.UsernameOrEmail) ||
                    u.Correo == model.UsernameOrEmail);

            if (usuario == null || usuario.CredencialAcceso == null)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return View(model);
            }

            var cred = usuario.CredencialAcceso;

            var politica = await _context.PoliticasSeguridad.FirstOrDefaultAsync()
                           ?? new PoliticaSeguridad();

            var ahoraUtc = DateTime.UtcNow;

            if (cred.BloqueadaHasta.HasValue && cred.BloqueadaHasta.Value > ahoraUtc)
            {
                ModelState.AddModelError(string.Empty,
                    $"La cuenta está bloqueada hasta {cred.BloqueadaHasta.Value.ToLocalTime():g}.");
                return View(model);
            }

            var passwordValida = PasswordHelper.VerifyPassword(model.Password, cred.PasswordHash);
            if (!passwordValida)
            {
                cred.IntentosFallidos++;

                if (cred.IntentosFallidos >= politica.IntentosMaximosFallidos)
                {
                    cred.BloqueadaHasta = ahoraUtc.AddMinutes(politica.MinutosBloqueo);
                    usuario.EstadoCuenta = "Bloqueado";
                }

                await RegistrarAuditoria(usuario, "LoginFallido", "Contraseña incorrecta");

                await _context.SaveChangesAsync();
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return View(model);
            }

            if (usuario.EstadoCuenta == "Pendiente")
            {
                ModelState.AddModelError(string.Empty, "Su cuenta está pendiente de aprobación.");
                return View(model);
            }

            if (usuario.EstadoCuenta == "Desactivado" || usuario.EstadoCuenta == "Bloqueado")
            {
                ModelState.AddModelError(string.Empty, "Su cuenta no está habilitada para acceder.");
                return View(model);
            }

            cred.IntentosFallidos = 0;
            cred.BloqueadaHasta = null;

            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{usuario.Nombres} {usuario.Apellidos}"),
                new Claim(ClaimTypes.Role, usuario.Rol.Nombre),
                new Claim("TipoUsuario", usuario.TipoUsuario)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.Recordarme,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            await RegistrarAuditoria(usuario, "LoginExitoso", "Inicio de sesión correcto");

            if (cred.RequiereCambioPrimerLogin)
            {
                return RedirectToAction("CambiarPasswordPrimerLogin", "Auth");
            }

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("RedirigirPorRol");
        }

        [Authorize]
        public IActionResult RedirigirPorRol()
        {
            if (User.IsInRole("Director"))
                return RedirectToAction("Director", "Dashboard");

            if (User.IsInRole("Secretaria"))
                return RedirectToAction("Secretaria", "Dashboard");

            if (User.IsInRole("Docente"))
                return RedirectToAction("Docente", "Dashboard");

            if (User.IsInRole("Estudiante"))
                return RedirectToAction("Estudiante", "Dashboard");

            if (User.IsInRole("Padre"))
                return RedirectToAction("Padre", "Dashboard");

            if (User.IsInRole("Regente"))
                return RedirectToAction("Regente", "Dashboard");

            if (User.IsInRole("Bibliotecario"))
                return RedirectToAction("Bibliotecario", "Dashboard");

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccesoDenegado()
        {
            return View();
        }

        private async Task RegistrarAuditoria(Usuario usuario, string accion, string detalle)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";

            var registro = new RegistroAuditoriaUsuario
            {
                UsuarioAfectadoId = usuario.Id,
                ActorId = usuario.Id,
                FechaHora = DateTime.UtcNow,
                Accion = accion,
                Detalle = $"{detalle} | IP: {ip}",
                Origen = "Auth/Login"
            };

            _context.RegistrosAuditoriaUsuarios.Add(registro);
            await _context.SaveChangesAsync();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CambiarPasswordPrimerLogin()
        {
            var usuario = await ObtenerUsuarioActualAsync();

            if (usuario == null || usuario.CredencialAcceso == null)
                return RedirectToAction("Login");

            if (!usuario.CredencialAcceso.RequiereCambioPrimerLogin)
                return RedirectToAction("RedirigirPorRol");

            var model = new CambiarPasswordPrimerLoginViewModel();
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPasswordPrimerLogin(CambiarPasswordPrimerLoginViewModel model)
        {
            var usuario = await ObtenerUsuarioActualAsync();

            if (usuario == null || usuario.CredencialAcceso == null)
                return RedirectToAction("Login");

            var cred = usuario.CredencialAcceso;

            if (!cred.RequiereCambioPrimerLogin)
                return RedirectToAction("RedirigirPorRol");

            if (!ModelState.IsValid)
                return View(model);

            if (!PasswordHelper.VerifyPassword(model.PasswordActual, cred.PasswordHash))
            {
                ModelState.AddModelError(nameof(model.PasswordActual), "La contraseña actual no es correcta.");
                return View(model);
            }

            if (!EsPasswordSegura(model.NuevaPassword, out var mensajeError))
            {
                ModelState.AddModelError(nameof(model.NuevaPassword), mensajeError);
                return View(model);
            }

            cred.PasswordHash = PasswordHelper.HashPassword(model.NuevaPassword);
            cred.EsTemporal = false;
            cred.RequiereCambioPrimerLogin = false;
            cred.IntentosFallidos = 0;
            cred.BloqueadaHasta = null;
            cred.FechaExpiracion = null;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "La contraseña se cambió correctamente.";
            return RedirectToAction("RedirigirPorRol");
        }

        private async Task<Usuario?> ObtenerUsuarioActualAsync()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var id))
                return null;

            return await _context.Usuarios
                .Include(u => u.CredencialAcceso)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        private bool EsPasswordSegura(string password, out string mensaje)
        {
            if (string.IsNullOrEmpty(password))
            {
                mensaje = "La contraseña no puede estar vacía.";
                return false;
            }

            if (password.Length < 8)
            {
                mensaje = "La contraseña debe tener al menos 8 caracteres.";
                return false;
            }

            if (!password.Any(char.IsLower))
            {
                mensaje = "La contraseña debe contener al menos una letra minúscula.";
                return false;
            }

            if (!password.Any(char.IsUpper))
            {
                mensaje = "La contraseña debe contener al menos una letra mayúscula.";
                return false;
            }

            if (!password.Any(char.IsDigit))
            {
                mensaje = "La contraseña debe contener al menos un número.";
                return false;
            }

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
            {
                mensaje = "La contraseña debe contener al menos un símbolo (no letra ni número).";
                return false;
            }

            mensaje = string.Empty;
            return true;
        }
    }
}
