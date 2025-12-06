using GestionLaboresAcademicas.Data;
using GestionLaboresAcademicas.Models;
using GestionLaboresAcademicas.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace GestionLaboresAcademicas.Controllers
{
    [Authorize(Roles = "Director")]
    public class AprobacionRolesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AprobacionRolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var solicitudes = await _context.SolicitudesAprobacionRoles
                .Include(s => s.UsuarioSolicitado)
                .Include(s => s.RolSolicitado)
                .Where(s => s.Estado == "Pendiente")
                .OrderBy(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            var solicitud = await _context.SolicitudesAprobacionRoles
                .Include(s => s.UsuarioSolicitado)
                .Include(s => s.RolSolicitado)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound();

            if (solicitud.Estado != "Pendiente")
                return RedirectToAction(nameof(Index));

            solicitud.Estado = "Aprobado";
            solicitud.FechaRespuesta = DateTime.UtcNow;

            var usuario = solicitud.UsuarioSolicitado;
            usuario.EstadoCuenta = "Habilitado";

            await RegistrarAuditoriaRol(usuario, solicitud, aprobado: true, motivo: null);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Rechazar(int id)
        {
            var solicitud = await _context.SolicitudesAprobacionRoles
                .Include(s => s.UsuarioSolicitado)
                .Include(s => s.RolSolicitado)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null || solicitud.Estado != "Pendiente")
                return NotFound();

            var model = new RechazarSolicitudRolViewModel
            {
                Id = solicitud.Id,
                UsuarioNombre = $"{solicitud.UsuarioSolicitado.Nombres} {solicitud.UsuarioSolicitado.Apellidos}",
                RolNombre = solicitud.RolSolicitado.Nombre
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Director")]
        public async Task<IActionResult> Rechazar(RechazarSolicitudRolViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var sol = await _context.SolicitudesAprobacionRoles
                    .Include(s => s.UsuarioSolicitado)
                    .Include(s => s.RolSolicitado)
                    .FirstOrDefaultAsync(s => s.Id == model.Id);

                if (sol != null)
                {
                    model.UsuarioNombre = $"{sol.UsuarioSolicitado.Nombres} {sol.UsuarioSolicitado.Apellidos}";
                    model.RolNombre = sol.RolSolicitado.Nombre;
                }

                return View(model);
            }

            var solicitud = await _context.SolicitudesAprobacionRoles
                .Include(s => s.UsuarioSolicitado)
                .Include(s => s.RolSolicitado)
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (solicitud == null || solicitud.Estado != "Pendiente")
                return NotFound();

            solicitud.Estado = "Rechazado";
            solicitud.FechaRespuesta = DateTime.UtcNow;
            solicitud.MotivoRechazo = model.Motivo;

            var usuario = solicitud.UsuarioSolicitado;
            usuario.EstadoCuenta = "Desactivado";

            await RegistrarAuditoriaRol(usuario, solicitud, aprobado: false, motivo: model.Motivo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private async Task RegistrarAuditoriaRol(Usuario usuarioAfectado, SolicitudAprobacionRol solicitud, bool aprobado, string? motivo)
        {
            var actorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? actorId = null;
            if (int.TryParse(actorIdString, out var parsed))
            {
                actorId = parsed;
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
            var accion = aprobado ? "AprobarRol" : "RechazarRol";
            var detalle = new StringBuilder()
                .Append($"Rol {solicitud.RolSolicitado.Nombre} para {usuarioAfectado.Nombres} {usuarioAfectado.Apellidos} ")
                .Append(aprobado ? "APROBADO" : "RECHAZADO");

            if (!string.IsNullOrWhiteSpace(motivo))
                detalle.Append($". Motivo: {motivo}");

            detalle.Append($". IP: {ip}");

            var registro = new RegistroAuditoriaUsuario
            {
                UsuarioAfectadoId = usuarioAfectado.Id,
                ActorId = actorId,
                FechaHora = DateTime.UtcNow,
                Accion = accion,
                Detalle = detalle.ToString(),
                Origen = "AprobacionRoles"
            };

            _context.RegistrosAuditoriaUsuarios.Add(registro);
            await _context.SaveChangesAsync();
        }
    }
}
