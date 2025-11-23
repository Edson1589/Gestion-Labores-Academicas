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
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

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
            // Siempre recargamos el combo
            await CargarTiposUsuarioAsync();

            // Validaciones de DataAnnotations
            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            // Validaciones de unicidad (CI, Correo, NombreUsuario)
            if (await _context.Usuarios.AnyAsync(u => u.CI == usuario.CI))
            {
                ModelState.AddModelError("CI", "Ya existe un usuario registrado con este CI.");
            }

            if (await _context.Usuarios.AnyAsync(u => u.Correo == usuario.Correo))
            {
                ModelState.AddModelError("Correo", "Ya existe un usuario registrado con este correo.");
            }

            if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario))
            {
                ModelState.AddModelError("NombreUsuario", "Ya existe un usuario con este nombre de usuario.");

                // Proponemos un usuario alternativo
                ViewBag.SugerenciaUsuario = GenerarNombreUsuarioSugerido(usuario);
            }

            // Si hay errores, regresamos al formulario
            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            usuario.FechaRegistro = DateTime.UtcNow;

            _context.Add(usuario);
            await _context.SaveChangesAsync();

            // Criterio: mostrar un resumen → reutilizamos Details
            return RedirectToAction(nameof(Details), new { id = usuario.Id });
        }

        // Métodos Edit/Delete los puedes dejar scaffold por ahora...

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

        // Helpers privados

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

        private string GenerarNombreUsuarioSugerido(Usuario usuario)
        {
            // Ejemplo simple: primera letra del nombre + primer apellido + número aleatorio
            var nombres = (usuario.Nombres ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var apellidos = (usuario.Apellidos ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var inicial = nombres.Length > 0 ? nombres[0][0].ToString().ToLower() : "u";
            var apellidoBase = apellidos.Length > 0 ? apellidos[0].ToLower() : "usuario";

            var baseUsuario = inicial + apellidoBase;

            var random = new Random();
            var sufijo = random.Next(100, 999); // 3 dígitos

            return $"{baseUsuario}{sufijo}";
        }
    }
}
