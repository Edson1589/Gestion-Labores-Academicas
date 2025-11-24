using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionLaboresAcademicas.Data;
using GestionLaboresAcademicas.Models;

namespace GestionLaboresAcademicas.Controllers
{
    // [Authorize(Roles = "Secretaria")]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random = new Random();

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.TipoUsuario)
                .OrderBy(u => u.Apellidos)
                .ThenBy(u => u.Nombres)
                .ToListAsync();

            return View(usuarios);
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.TipoUsuario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // GET: Usuarios/Create
        public async Task<IActionResult> Create()
        {
            await CargarTiposUsuarioAsync();
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            await CargarTiposUsuarioAsync();

            // Estos campos los genera el sistema, no vienen del formulario.
            ModelState.Remove(nameof(Usuario.NombreUsuario));
            ModelState.Remove(nameof(Usuario.PasswordTemporal));
            ModelState.Remove(nameof(Usuario.EstadoCuenta));
            ModelState.Remove(nameof(Usuario.DebeCambiarPassword));
            ModelState.Remove(nameof(Usuario.CreadoPor));
            ModelState.Remove(nameof(Usuario.OrigenRegistro));

            // 1) Validar campos obligatorios y formatos
            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            // 2) Validar duplicados (CI, Correo)
            if (await _context.Usuarios.AnyAsync(u => u.CI == usuario.CI))
            {
                ModelState.AddModelError("CI", "Ya existe un usuario registrado con este CI.");
            }

            if (await _context.Usuarios.AnyAsync(u => u.Correo == usuario.Correo))
            {
                ModelState.AddModelError("Correo", "Ya existe un usuario registrado con este correo.");
            }

            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            // 3) Cargar tipo de usuario para decidir estado de cuenta
            var tipoUsuario = await _context.TiposUsuario.FindAsync(usuario.TipoUsuarioId);
            if (tipoUsuario == null)
            {
                ModelState.AddModelError("TipoUsuarioId", "Debe seleccionar un tipo de usuario válido.");
                return View(usuario);
            }

            // 4) Generar nombre de usuario único (política: inicial nombre + apellido)
            usuario.NombreUsuario = await GenerarNombreUsuarioUnicoAsync(usuario);

            // 5) Generar contraseña temporal con complejidad
            usuario.PasswordTemporal = GenerarPasswordTemporal();
            usuario.DebeCambiarPassword = true;

            // 6) Estado de cuenta según rol
            bool esRolSensible = EsRolSensible(tipoUsuario.Nombre);

            usuario.EstadoCuenta = esRolSensible
                ? EstadoCuenta.PendienteAprobacion
                : EstadoCuenta.Habilitada;

            // 7) Trazabilidad
            usuario.FechaRegistro = DateTime.UtcNow;
            usuario.CreadoPor = User?.Identity?.Name ?? "secretaria.demo";
            usuario.OrigenRegistro = "Gestión de usuarios / Registrar usuario";

            _context.Add(usuario);
            await _context.SaveChangesAsync();

            // 8) "Notificar" al Director (simulación para la demo)
            if (esRolSensible)
            {
                TempData["NotificacionDirector"] =
                    $"La cuenta de {usuario.Nombres} {usuario.Apellidos} ({tipoUsuario.Nombre}) " +
                    "se ha creado como 'Pendiente de aprobación'. Se ha notificado al Director.";
            }

            // Mostrar resumen del registro
            return RedirectToAction(nameof(Details), new { id = usuario.Id });
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound();

            await CargarTiposUsuarioAsync(usuario.TipoUsuarioId);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Usuario usuario)
        {
            if (id != usuario.Id)
                return NotFound();

            await CargarTiposUsuarioAsync(usuario.TipoUsuarioId);

            if (!ModelState.IsValid)
                return View(usuario);

            try
            {
                _context.Update(usuario);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(usuario.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Details), new { id = usuario.Id });
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.TipoUsuario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }

        private async Task CargarTiposUsuarioAsync(int? seleccionado = null)
        {
            var tipos = await _context.TiposUsuario
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            ViewBag.TipoUsuarioId = new SelectList(tipos, "Id", "Nombre", seleccionado);
        }

        // Define qué roles son "sensibles" según el nombre del tipo de usuario
        private bool EsRolSensible(string nombreTipo)
        {
            if (string.IsNullOrWhiteSpace(nombreTipo))
                return false;

            nombreTipo = nombreTipo.ToLower();

            // Ajusta esta política si quieres incluir/excluir otros
            return nombreTipo.Contains("director")
                   || nombreTipo.Contains("regente")
                   || nombreTipo.Contains("bibliotec");
        }

        // Genera un nombre de usuario base (inicial nombre + primer apellido)
        private string GenerarBaseNombreUsuario(Usuario usuario)
        {
            var nombres = (usuario.Nombres ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var apellidos = (usuario.Apellidos ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var inicial = nombres.Length > 0 ? char.ToLower(nombres[0][0]) : 'u';
            var apellidoBase = apellidos.Length > 0 ? apellidos[0].ToLower() : "usuario";

            return $"{inicial}{apellidoBase}";
        }

        // Genera un nombre de usuario único en la base de datos
        private async Task<string> GenerarNombreUsuarioUnicoAsync(Usuario usuario)
        {
            var baseUsuario = GenerarBaseNombreUsuario(usuario);
            var nombre = baseUsuario;
            int intentos = 0;

            while (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombre))
            {
                intentos++;
                nombre = $"{baseUsuario}{_random.Next(100, 999)}";

                if (intentos > 20)
                {
                    // Fallback por si hay demasiadas colisiones
                    nombre = $"{baseUsuario}{DateTime.UtcNow.Ticks % 10000}";
                }
            }

            return nombre;
        }

        // Genera una contraseña temporal con mayúsculas, minúsculas, números y símbolos
        private string GenerarPasswordTemporal()
        {
            const int length = 10;
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string symbols = "!@#$%^&*()-_+";

            var chars = new System.Collections.Generic.List<char>
            {
                lower[_random.Next(lower.Length)],
                upper[_random.Next(upper.Length)],
                digits[_random.Next(digits.Length)],
                symbols[_random.Next(symbols.Length)]
            };

            var all = lower + upper + digits + symbols;

            for (int i = chars.Count; i < length; i++)
            {
                chars.Add(all[_random.Next(all.Length)]);
            }

            // Mezclamos los caracteres para que no queden siempre en el mismo orden
            return new string(chars.OrderBy(_ => _random.Next()).ToArray());
        }
    }
}
