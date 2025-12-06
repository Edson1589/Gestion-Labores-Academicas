using GestionLaboresAcademicas.Data;
using GestionLaboresAcademicas.Models;
using GestionLaboresAcademicas.Models.ViewModels;
using GestionLaboresAcademicas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace GestionLaboresAcademicas.Controllers
{
    [Authorize(Roles = "Secretaria")]
    public class GestionUsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ServicioGestionUsuarios _servicioGestionUsuarios;

        public GestionUsuariosController(
            ApplicationDbContext context,
            IConfiguration configuration,
            ServicioGestionUsuarios servicioGestionUsuarios)
        {
            _context = context;
            _configuration = configuration;
            _servicioGestionUsuarios = servicioGestionUsuarios;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? termino)
        {
            IQueryable<Usuario> query = _context.Usuarios
                .Include(u => u.Rol);

            if (!string.IsNullOrWhiteSpace(termino))
            {
                var texto = termino.Trim().ToLower();
                query = query.Where(u =>
                    u.Nombres.ToLower().Contains(texto) ||
                    u.Apellidos.ToLower().Contains(texto) ||
                    u.DocumentoCI.ToLower().Contains(texto) ||
                    u.Correo.ToLower().Contains(texto));
            }

            var resultados = await query
                .OrderBy(u => u.Nombres)
                .ThenBy(u => u.Apellidos)
                .Take(50)
                .ToListAsync();

            ViewBag.Termino = termino;

            return View(resultados);
        }

        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            var model = new RegistrarUsuarioViewModel
            {
                TiposUsuario = ObtenerTiposUsuario(),
                Cursos = await ObtenerCursosActivosAsync(),
                Asignaturas = await ObtenerAsignaturasAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistrarUsuarioViewModel model, string? accion)
        {
            model.TiposUsuario = ObtenerTiposUsuario();
            model.Cursos = await ObtenerCursosActivosAsync();
            model.Asignaturas = await ObtenerAsignaturasAsync();

            if (accion == "buscarEstudiantes" && model.TipoUsuario == "Padre")
            {
                await CargarEstudiantesDisponiblesAsync(model);
                return View(model);
            }

            accion ??= "guardar";

            if (accion != "guardar")
            {
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existeCI = await _context.Usuarios.AnyAsync(u => u.DocumentoCI == model.DocumentoCI);
            if (existeCI)
            {
                ModelState.AddModelError(nameof(model.DocumentoCI), "El CI ya está registrado.");
            }

            var existeCorreo = await _context.Usuarios.AnyAsync(u => u.Correo == model.Correo);
            if (existeCorreo)
            {
                ModelState.AddModelError(nameof(model.Correo), "El correo ya está registrado.");
            }

            if (string.IsNullOrWhiteSpace(model.TipoUsuario))
            {
                ModelState.AddModelError(nameof(model.TipoUsuario), "El tipo de usuario es obligatorio.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == model.TipoUsuario);
            if (rol == null)
            {
                ModelState.AddModelError(nameof(model.TipoUsuario), "El tipo de usuario seleccionado no es válido.");
                return View(model);
            }

            bool pendienteAsignacionCurso = false;
            int? cursoIdSeleccionado = null;

            if (model.TipoUsuario == "Estudiante")
            {
                var cicloVigente = _configuration["PerfilCiclos:CicloVigente"];
                var fechaInicioCambiosStr = _configuration["PerfilCiclos:FechaInicioCambios"];
                var fechaFinCambiosStr = _configuration["PerfilCiclos:FechaFinCambios"];

                DateTime.TryParse(fechaInicioCambiosStr, out var fechaInicioCambios);
                DateTime.TryParse(fechaFinCambiosStr, out var fechaFinCambios);
                var hoy = DateTime.Today;

                var gestionActivaPorFecha = hoy >= fechaInicioCambios && hoy <= fechaFinCambios;

                if (model.CursoId.HasValue)
                {
                    var curso = await _context.Cursos
                        .FirstOrDefaultAsync(c => c.Id == model.CursoId.Value);

                    var cursoExiste = curso != null;
                    var gestionCoincide = cursoExiste &&
                                          string.Equals(curso!.Gestion, cicloVigente, StringComparison.OrdinalIgnoreCase);
                    var gestionActiva = cursoExiste && gestionCoincide && gestionActivaPorFecha;

                    if (!cursoExiste || !gestionActiva)
                    {
                        if (!model.GuardarComoPendienteCurso)
                        {
                            ModelState.AddModelError(nameof(model.CursoId),
                                "El curso/paralelo seleccionado no existe o la gestión no está activa. " +
                                "Puede corregir el curso o marcar 'Guardar estudiante como pendiente de asignación de curso'.");
                        }
                        else
                        {
                            pendienteAsignacionCurso = true;
                            cursoIdSeleccionado = null;
                        }
                    }
                    else
                    {
                        cursoIdSeleccionado = curso!.Id;
                        pendienteAsignacionCurso = false;
                    }
                }
                else
                {
                    if (!model.GuardarComoPendienteCurso)
                    {
                        ModelState.AddModelError(nameof(model.CursoId),
                            "Debe seleccionar un curso/paralelo o marcar 'Guardar estudiante como pendiente de asignación de curso'.");
                    }
                    else
                    {
                        pendienteAsignacionCurso = true;
                        cursoIdSeleccionado = null;
                    }
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }
            }

            List<int> idsEstudiantesSeleccionados = new();
            bool padrePendienteVinculo = false;

            if (model.TipoUsuario == "Padre")
            {
                idsEstudiantesSeleccionados = model.EstudiantesDisponibles?
                    .Where(e => e.Seleccionado)
                    .Select(e => e.Id)
                    .Distinct()
                    .ToList() ?? new List<int>();

                if (idsEstudiantesSeleccionados.Count == 0 && !model.GuardarPadreSinVinculo)
                {
                    ModelState.AddModelError(string.Empty,
                        "Debe seleccionar al menos un estudiante o marcar 'Guardar padre sin vínculo (pendiente de vincular)'.");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                padrePendienteVinculo = model.GuardarPadreSinVinculo && idsEstudiantesSeleccionados.Count == 0;
            }

            bool pendienteAsignaturas = false;
            List<int> asignaturasSeleccionadas = new();

            if (model.TipoUsuario == "Docente")
            {
                if (string.IsNullOrWhiteSpace(model.ItemDocente))
                {
                    ModelState.AddModelError(nameof(model.ItemDocente),
                        "El ítem/código interno es obligatorio para docentes.");
                }
                else
                {
                    var existeItem = await _context.Usuarios.AnyAsync(u =>
                        u.TipoUsuario == "Docente" &&
                        u.ItemDocente == model.ItemDocente);

                    if (existeItem)
                    {
                        ModelState.AddModelError(nameof(model.ItemDocente),
                            "El ítem/código interno ya está registrado para otro docente.");
                    }
                }

                asignaturasSeleccionadas = model.AsignaturasSeleccionadas?
                    .Distinct()
                    .ToList() ?? new List<int>();

                if (asignaturasSeleccionadas.Any())
                {
                    var countValidas = await _context.Asignaturas
                        .CountAsync(a => asignaturasSeleccionadas.Contains(a.Id));

                    if (countValidas != asignaturasSeleccionadas.Count)
                    {
                        ModelState.AddModelError(nameof(model.AsignaturasSeleccionadas),
                            "Algunas asignaturas seleccionadas no son válidas.");
                    }

                    pendienteAsignaturas = false;
                }
                else
                {
                    pendienteAsignaturas = true;
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }
            }

            var estadoCuentaInicial = rol.RequiereAprobacion ? "Pendiente" : "Habilitado";

            var usuario = new Usuario
            {
                Nombres = model.Nombres,
                Apellidos = model.Apellidos,
                DocumentoCI = model.DocumentoCI,
                FechaNacimiento = model.FechaNacimiento!.Value,
                Correo = model.Correo,
                Telefono = model.Telefono,
                TipoUsuario = model.TipoUsuario,
                EstadoCuenta = estadoCuentaInicial,
                RolId = rol.Id,
                CursoId = cursoIdSeleccionado,
                PendienteAsignacionCurso = pendienteAsignacionCurso,
                PendienteVinculoEstudiantes = padrePendienteVinculo,
                ItemDocente = model.TipoUsuario == "Docente" ? model.ItemDocente : null,
                PendienteAsignarAsignaturas = model.TipoUsuario == "Docente" ? pendienteAsignaturas : false
            };

            var (credencial, passwordTemporal) = await _servicioGestionUsuarios.GenerarCredencialesInicialesAsync(usuario);
            usuario.CredencialAcceso = credencial;

            if (rol.RequiereAprobacion)
            {
                var solicitud = _servicioGestionUsuarios.CrearSolicitudAprobacionRol(usuario, rol);
                _context.SolicitudesAprobacionRoles.Add(solicitud);
            }

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            if (model.TipoUsuario == "Padre" && idsEstudiantesSeleccionados.Any())
            {
                var vinculos = idsEstudiantesSeleccionados.Select(idEst => new VinculoPadreEstudiante
                {
                    PadreId = usuario.Id,
                    EstudianteId = idEst,
                    Relacion = "padre",
                    EsTutorLegal = true
                });

                _context.VinculosPadreEstudiante.AddRange(vinculos);

                usuario.PendienteVinculoEstudiantes = false;

                await _context.SaveChangesAsync();
            }

            if (usuario.TipoUsuario == "Docente" && asignaturasSeleccionadas.Any())
            {
                var asignaturas = await _context.Asignaturas
                    .Where(a => asignaturasSeleccionadas.Contains(a.Id))
                    .ToListAsync();

                foreach (var asig in asignaturas)
                {
                    usuario.Asignaturas.Add(asig);
                }

                usuario.PendienteAsignarAsignaturas = false;
                await _context.SaveChangesAsync();
            }

            await RegistrarAuditoriaCreacion(usuario);
            TempData["PasswordTemporal"] = passwordTemporal;

            return RedirectToAction(nameof(Resumen), new { id = usuario.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Curso)
                .Include(u => u.Asignaturas)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound();

            var model = new EditarUsuarioViewModel
            {
                Id = usuario.Id,
                TipoUsuario = usuario.TipoUsuario,
                EstadoCuenta = usuario.EstadoCuenta,
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                DocumentoCI = usuario.DocumentoCI,
                FechaNacimiento = usuario.FechaNacimiento,
                Correo = usuario.Correo,
                Telefono = usuario.Telefono,
                CursoId = usuario.CursoId,
                PendienteAsignacionCurso = usuario.PendienteAsignacionCurso,
                ItemDocente = usuario.ItemDocente,
                PendienteAsignarAsignaturas = usuario.PendienteAsignarAsignaturas,
                AsignaturasSeleccionadas = usuario.Asignaturas.Select(a => a.Id).ToList(),
                Cursos = await ObtenerCursosActivosAsync(),
                Asignaturas = await ObtenerAsignaturasAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarUsuarioViewModel model)
        {
            model.Cursos = await ObtenerCursosActivosAsync();
            model.Asignaturas = await ObtenerAsignaturasAsync();

            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _context.Usuarios
                .Include(u => u.Asignaturas)
                .FirstOrDefaultAsync(u => u.Id == model.Id);

            if (usuario == null)
                return NotFound();

            var existeCI = await _context.Usuarios
                .AnyAsync(u => u.Id != model.Id && u.DocumentoCI == model.DocumentoCI);
            if (existeCI)
                ModelState.AddModelError(nameof(model.DocumentoCI), "El CI ya está registrado para otro usuario.");

            var existeCorreo = await _context.Usuarios
                .AnyAsync(u => u.Id != model.Id && u.Correo == model.Correo);
            if (existeCorreo)
                ModelState.AddModelError(nameof(model.Correo), "El correo ya está registrado para otro usuario.");

            int? nuevoCursoId = usuario.CursoId;
            bool nuevoPendienteCurso = usuario.PendienteAsignacionCurso;

            if (usuario.TipoUsuario == "Estudiante")
            {
                var cicloVigente = _configuration["PerfilCiclos:CicloVigente"];
                var fechaInicioCambiosStr = _configuration["PerfilCiclos:FechaInicioCambios"];
                var fechaFinCambiosStr = _configuration["PerfilCiclos:FechaFinCambios"];

                DateTime.TryParse(fechaInicioCambiosStr, out var fechaInicioCambios);
                DateTime.TryParse(fechaFinCambiosStr, out var fechaFinCambios);
                var hoy = DateTime.Today;
                var gestionActivaPorFecha = hoy >= fechaInicioCambios && hoy <= fechaFinCambios;

                if (model.CursoId.HasValue)
                {
                    var curso = await _context.Cursos
                        .FirstOrDefaultAsync(c => c.Id == model.CursoId.Value);

                    var cursoExiste = curso != null;
                    var gestionCoincide = cursoExiste &&
                                          string.Equals(curso!.Gestion, cicloVigente, StringComparison.OrdinalIgnoreCase);
                    var gestionActiva = cursoExiste && gestionCoincide && gestionActivaPorFecha;

                    if (!cursoExiste || !gestionActiva)
                    {
                        ModelState.AddModelError(nameof(model.CursoId),
                            "El curso/paralelo seleccionado no existe o la gestión no está activa.");
                    }
                    else
                    {
                        nuevoCursoId = curso!.Id;
                        nuevoPendienteCurso = false;
                    }
                }
                else
                {
                    nuevoCursoId = null;
                    nuevoPendienteCurso = true;
                }
            }

            string? nuevoItemDocente = usuario.ItemDocente;
            bool nuevoPendienteAsignaturas = usuario.PendienteAsignarAsignaturas;
            List<int> nuevasAsignaturasIds = usuario.Asignaturas.Select(a => a.Id).ToList();

            if (usuario.TipoUsuario == "Docente")
            {
                if (string.IsNullOrWhiteSpace(model.ItemDocente))
                {
                    ModelState.AddModelError(nameof(model.ItemDocente),
                        "El ítem/código interno es obligatorio para docentes.");
                }
                else
                {
                    var existeItem = await _context.Usuarios.AnyAsync(u =>
                        u.Id != model.Id &&
                        u.TipoUsuario == "Docente" &&
                        u.ItemDocente == model.ItemDocente);

                    if (existeItem)
                    {
                        ModelState.AddModelError(nameof(model.ItemDocente),
                            "El ítem/código interno ya está registrado para otro docente.");
                    }
                }

                nuevasAsignaturasIds = model.AsignaturasSeleccionadas?
                    .Distinct()
                    .ToList() ?? new List<int>();

                if (nuevasAsignaturasIds.Any())
                {
                    var countValidas = await _context.Asignaturas
                        .CountAsync(a => nuevasAsignaturasIds.Contains(a.Id));

                    if (countValidas != nuevasAsignaturasIds.Count)
                    {
                        ModelState.AddModelError(nameof(model.AsignaturasSeleccionadas),
                            "Algunas asignaturas seleccionadas no son válidas.");
                    }

                    nuevoPendienteAsignaturas = false;
                }
                else
                {
                    nuevoPendienteAsignaturas = true;
                }

                nuevoItemDocente = model.ItemDocente;
            }

            if (!ModelState.IsValid)
                return View(model);

            var cambios = new List<string>();

            void AgregarCambio(string campo, string? antes, string? despues)
            {
                if (antes != despues)
                    cambios.Add($"{campo}: '{antes}' → '{despues}'");
            }

            AgregarCambio("Nombres", usuario.Nombres, model.Nombres);
            AgregarCambio("Apellidos", usuario.Apellidos, model.Apellidos);
            AgregarCambio("DocumentoCI", usuario.DocumentoCI, model.DocumentoCI);
            AgregarCambio("FechaNacimiento",
                usuario.FechaNacimiento.ToShortDateString(),
                model.FechaNacimiento!.Value.ToShortDateString());
            AgregarCambio("Correo", usuario.Correo, model.Correo);
            AgregarCambio("Telefono", usuario.Telefono, model.Telefono);

            if (usuario.TipoUsuario == "Estudiante")
            {
                var cursoAntes = usuario.CursoId?.ToString() ?? "(sin curso)";
                var cursoDespues = nuevoCursoId?.ToString() ?? "(sin curso)";
                AgregarCambio("CursoId", cursoAntes, cursoDespues);
                AgregarCambio("PendienteAsignacionCurso",
                    usuario.PendienteAsignacionCurso.ToString(),
                    nuevoPendienteCurso.ToString());
            }

            if (usuario.TipoUsuario == "Docente")
            {
                AgregarCambio("ItemDocente", usuario.ItemDocente, nuevoItemDocente);

                var antesIds = string.Join(",", usuario.Asignaturas.Select(a => a.Id));
                var despuesIds = string.Join(",", nuevasAsignaturasIds);
                AgregarCambio("Asignaturas", antesIds, despuesIds);

                AgregarCambio("PendienteAsignarAsignaturas",
                    usuario.PendienteAsignarAsignaturas.ToString(),
                    nuevoPendienteAsignaturas.ToString());
            }

            usuario.Nombres = model.Nombres;
            usuario.Apellidos = model.Apellidos;
            usuario.DocumentoCI = model.DocumentoCI;
            usuario.FechaNacimiento = model.FechaNacimiento!.Value;
            usuario.Correo = model.Correo;
            usuario.Telefono = model.Telefono;

            if (usuario.TipoUsuario == "Estudiante")
            {
                usuario.CursoId = nuevoCursoId;
                usuario.PendienteAsignacionCurso = nuevoPendienteCurso;
            }

            if (usuario.TipoUsuario == "Docente")
            {
                usuario.ItemDocente = nuevoItemDocente;
                usuario.PendienteAsignarAsignaturas = nuevoPendienteAsignaturas;

                usuario.Asignaturas.Clear();
                if (nuevasAsignaturasIds.Any())
                {
                    var nuevasAsignaturas = await _context.Asignaturas
                        .Where(a => nuevasAsignaturasIds.Contains(a.Id))
                        .ToListAsync();

                    foreach (var asig in nuevasAsignaturas)
                        usuario.Asignaturas.Add(asig);
                }
            }

            await _context.SaveChangesAsync();

            if (cambios.Any())
                await RegistrarAuditoriaEdicion(usuario, cambios);

            return RedirectToAction(nameof(Resumen), new { id = usuario.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Resumen(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Curso)
                .Include(u => u.VinculosComoPadre).ThenInclude(v => v.Estudiante)
                .Include(u => u.Asignaturas)
                .Include(u => u.CredencialAcceso)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound();

            ViewBag.PasswordTemporal = TempData["PasswordTemporal"] as string;

            return View(usuario);
        }

        private List<SelectListItem> ObtenerTiposUsuario()
        {
            return _context.Roles
                .OrderBy(r => r.Nombre)
                .Select(r => new SelectListItem
                {
                    Value = r.Nombre,
                    Text = r.Nombre
                })
                .ToList();
        }

        private async Task<List<SelectListItem>> ObtenerCursosActivosAsync()
        {
            var query = _context.Cursos.AsQueryable();

            var cicloVigenteConfig = _configuration["PerfilCiclos:CicloVigente"];

            List<Curso> cursos;

            if (!string.IsNullOrWhiteSpace(cicloVigenteConfig))
            {
                var cicloNorm = cicloVigenteConfig.Trim().ToLower();

                cursos = await query
                    .Where(c => c.Gestion != null &&
                                c.Gestion.ToLower().Contains(cicloNorm))
                    .OrderBy(c => c.Nivel)
                    .ThenBy(c => c.Grado)
                    .ThenBy(c => c.Paralelo)
                    .ToListAsync();

                if (!cursos.Any())
                {
                    cursos = await query
                        .OrderBy(c => c.Nivel)
                        .ThenBy(c => c.Grado)
                        .ThenBy(c => c.Paralelo)
                        .ToListAsync();
                }
            }
            else
            {
                cursos = await query
                    .OrderBy(c => c.Nivel)
                    .ThenBy(c => c.Grado)
                    .ThenBy(c => c.Paralelo)
                    .ToListAsync();
            }

            var lista = cursos.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Nivel} {c.Grado} {c.Paralelo} - {c.Turno} ({c.Gestion})"
            }).ToList();

            return lista;
        }

        private async Task RegistrarAuditoriaCreacion(Usuario usuarioCreado)
        {
            var actorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? actorId = null;
            if (int.TryParse(actorIdString, out var parsed))
            {
                actorId = parsed;
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";

            var registro = new RegistroAuditoriaUsuario
            {
                UsuarioAfectadoId = usuarioCreado.Id,
                ActorId = actorId,
                FechaHora = DateTime.UtcNow,
                Accion = "Registrar",
                Detalle = $"Usuario {usuarioCreado.Nombres} {usuarioCreado.Apellidos} ({usuarioCreado.TipoUsuario}) creado. IP: {ip}",
                Origen = "GestionUsuarios/Registrar"
            };

            _context.RegistrosAuditoriaUsuarios.Add(registro);
            await _context.SaveChangesAsync();
        }

        private async Task CargarEstudiantesDisponiblesAsync(RegistrarUsuarioViewModel model)
        {
            var query = _context.Usuarios
                .Include(u => u.Curso)
                .Where(u => u.TipoUsuario == "Estudiante");

            if (!string.IsNullOrWhiteSpace(model.BuscarNombreOCI))
            {
                var texto = model.BuscarNombreOCI.Trim().ToLower();
                query = query.Where(u =>
                    u.Nombres.ToLower().Contains(texto) ||
                    u.Apellidos.ToLower().Contains(texto) ||
                    u.DocumentoCI.ToLower().Contains(texto));
            }

            if (model.BuscarCursoId.HasValue)
            {
                query = query.Where(u => u.CursoId == model.BuscarCursoId.Value);
            }

            var lista = await query
                .OrderBy(u => u.Nombres)
                .ThenBy(u => u.Apellidos)
                .Take(50)
                .ToListAsync();

            model.EstudiantesDisponibles = lista.Select(u => new EstudianteSeleccionViewModel
            {
                Id = u.Id,
                NombreCompleto = $"{u.Nombres} {u.Apellidos}",
                DocumentoCI = u.DocumentoCI,
                CursoNombre = u.Curso != null ? u.Curso.NombreDisplay : "Sin curso"
            }).ToList();
        }

        private async Task<List<SelectListItem>> ObtenerAsignaturasAsync()
        {
            var asignaturas = await _context.Asignaturas
                .OrderBy(a => a.Nombre)
                .ToListAsync();

            return asignaturas
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = string.IsNullOrEmpty(a.Area)
                        ? a.Nombre
                        : $"{a.Nombre} ({a.Area})"
                })
                .ToList();
        }

        private async Task RegistrarAuditoriaEdicion(Usuario usuario, IEnumerable<string> cambios)
        {
            var actorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? actorId = null;
            if (int.TryParse(actorIdString, out var parsed))
                actorId = parsed;

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";

            var detalle = "Cambios aplicados: " + string.Join("; ", cambios) + $". IP: {ip}";

            var registro = new RegistroAuditoriaUsuario
            {
                UsuarioAfectadoId = usuario.Id,
                ActorId = actorId,
                FechaHora = DateTime.UtcNow,
                Accion = "Editar",
                Detalle = detalle,
                Origen = "GestionUsuarios/Editar"
            };

            _context.RegistrosAuditoriaUsuarios.Add(registro);
            await _context.SaveChangesAsync();
        }

        [Authorize(Roles = "Secretaria,Director")]
        [HttpGet]
        public async Task<IActionResult> Historial(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound();

            var registros = await _context.RegistrosAuditoriaUsuarios
                .Include(r => r.Actor)
                .Where(r => r.UsuarioAfectadoId == id)
                .OrderByDescending(r => r.FechaHora)
                .ToListAsync();

            var model = new HistorialUsuarioViewModel
            {
                UsuarioId = usuario.Id,
                NombreCompleto = $"{usuario.Nombres} {usuario.Apellidos}",
                TipoUsuario = usuario.TipoUsuario,
                Registros = registros
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult DescargarPlantillaCargaMasiva()
        {
            var header = "TipoUsuario,Nombres,Apellidos,DocumentoCI,FechaNacimiento,Correo,Telefono,CursoId,HijosCI,ItemDocente,AsignaturasIds";
            var bytes = Encoding.UTF8.GetBytes(header + Environment.NewLine);
            var fileName = "plantilla_carga_masiva_usuarios.csv";
            return File(bytes, "text/csv", fileName);
        }

        [HttpGet]
        [Authorize(Roles = "Secretaria,Administrador")]
        public IActionResult CargaMasiva()
        {
            var model = new CargaMasivaUsuariosViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Secretaria,Administrador")]
        public async Task<IActionResult> CargaMasiva(CargaMasivaUsuariosViewModel model)
        {
            if (model.Archivo == null || model.Archivo.Length == 0)
            {
                ModelState.AddModelError(nameof(model.Archivo), "Debe seleccionar un archivo CSV.");
                return View(model);
            }

            var (validos, resultados) = await ParsearYValidarArchivoAsync(model.Archivo);

            model.Resultados = resultados;
            model.TotalRegistros = resultados.Count;
            model.RegistrosValidos = resultados.Count(r => r.EsValido);
            model.RegistrosInvalidos = resultados.Count(r => !r.EsValido);

            if (model.RegistrosValidos > 0)
            {
                model.DatosValidosJson = JsonSerializer.Serialize(validos);
            }

            return View(model);
        }

        private async Task<(List<UsuarioImportDto> validos, List<ResultadoFilaCargaMasiva> resultados)>
            ParsearYValidarArchivoAsync(IFormFile archivo)
        {
            var resultados = new List<ResultadoFilaCargaMasiva>();
            var validos = new List<UsuarioImportDto>();

            var cisEnArchivo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var correosEnArchivo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var stream = archivo.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            string? header = await reader.ReadLineAsync();
            int numeroLinea = 1;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                numeroLinea++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var cols = line.Split(',');

                if (cols.Length < 7)
                {
                    resultados.Add(new ResultadoFilaCargaMasiva
                    {
                        NumeroLinea = numeroLinea,
                        EsValido = false,
                        Mensaje = "La fila no tiene el número mínimo de columnas requeridas."
                    });
                    continue;
                }

                string tipoUsuario = cols[0].Trim();
                string nombres = cols[1].Trim();
                string apellidos = cols[2].Trim();
                string documentoCI = cols[3].Trim();
                string fechaNacStr = cols[4].Trim();
                string correo = cols[5].Trim();
                string telefono = cols[6].Trim();

                string? cursoIdStr = cols.Length > 7 ? cols[7].Trim() : null;
                string? hijosCI = cols.Length > 8 ? cols[8].Trim() : null;
                string? itemDocente = cols.Length > 9 ? cols[9].Trim() : null;
                string? asignaturasIdsStr = cols.Length > 10 ? cols[10].Trim() : null;

                var errores = new List<string>();

                if (string.IsNullOrWhiteSpace(tipoUsuario))
                    errores.Add("TipoUsuario es obligatorio.");

                if (string.IsNullOrWhiteSpace(nombres))
                    errores.Add("Nombres es obligatorio.");

                if (string.IsNullOrWhiteSpace(apellidos))
                    errores.Add("Apellidos es obligatorio.");

                if (string.IsNullOrWhiteSpace(documentoCI))
                    errores.Add("DocumentoCI es obligatorio.");

                DateTime? fechaNacimiento = null;
                if (string.IsNullOrWhiteSpace(fechaNacStr) ||
                    !DateTime.TryParse(fechaNacStr, out var fechaTmp))
                {
                    errores.Add("FechaNacimiento es obligatoria y debe tener formato válido (YYYY-MM-DD).");
                }
                else
                {
                    fechaNacimiento = fechaTmp;
                }

                if (string.IsNullOrWhiteSpace(correo))
                    errores.Add("Correo es obligatorio.");

                if (string.IsNullOrWhiteSpace(telefono))
                    errores.Add("Telefono es obligatorio.");

                var tiposValidos = new[]
                {
            "Director", "Secretaria", "Docente", "Estudiante",
            "Padre", "Regente", "Bibliotecario"
        };

                if (!tiposValidos.Contains(tipoUsuario))
                    errores.Add($"TipoUsuario '{tipoUsuario}' no es válido.");

                if (!string.IsNullOrEmpty(documentoCI))
                {
                    if (!cisEnArchivo.Add(documentoCI))
                        errores.Add($"El CI '{documentoCI}' está duplicado en el archivo.");
                }

                if (!string.IsNullOrEmpty(correo))
                {
                    if (!correosEnArchivo.Add(correo))
                        errores.Add($"El correo '{correo}' está duplicado en el archivo.");
                }

                int? cursoId = null;
                if (!string.IsNullOrEmpty(cursoIdStr))
                {
                    if (int.TryParse(cursoIdStr, out var parsedCursoId))
                        cursoId = parsedCursoId;
                    else
                        errores.Add("CursoId debe ser numérico.");
                }

                var asignaturasIds = new List<int>();
                if (!string.IsNullOrEmpty(asignaturasIdsStr))
                {
                    foreach (var part in asignaturasIdsStr.Split('|', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (int.TryParse(part.Trim(), out var asigId))
                            asignaturasIds.Add(asigId);
                        else
                            errores.Add($"AsignaturasIds contiene un valor no numérico: '{part}'.");
                    }
                }

                if (tipoUsuario == "Estudiante" && !cursoId.HasValue)
                    errores.Add("CursoId es obligatorio para Estudiante.");

                if (tipoUsuario == "Docente" && string.IsNullOrWhiteSpace(itemDocente))
                    errores.Add("ItemDocente es obligatorio para Docente.");

                var resultado = new ResultadoFilaCargaMasiva
                {
                    NumeroLinea = numeroLinea,
                    TipoUsuario = tipoUsuario,
                    Nombres = nombres,
                    Apellidos = apellidos,
                    DocumentoCI = documentoCI,
                    Correo = correo,
                    EsValido = !errores.Any(),
                    Mensaje = errores.Any() ? string.Join(" | ", errores) : "OK"
                };

                resultados.Add(resultado);

                if (!errores.Any())
                {
                    validos.Add(new UsuarioImportDto
                    {
                        NumeroLinea = numeroLinea,
                        TipoUsuario = tipoUsuario,
                        Nombres = nombres,
                        Apellidos = apellidos,
                        DocumentoCI = documentoCI,
                        FechaNacimiento = fechaNacimiento,
                        Correo = correo,
                        Telefono = telefono,
                        CursoId = cursoId,
                        HijosCI = string.IsNullOrWhiteSpace(hijosCI) ? null : hijosCI,
                        ItemDocente = string.IsNullOrWhiteSpace(itemDocente) ? null : itemDocente,
                        AsignaturasIds = asignaturasIds
                    });
                }
            }

            return (validos, resultados);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Secretaria,Administrador")]
        public async Task<IActionResult> ConfirmarCargaMasiva(CargaMasivaUsuariosViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.DatosValidosJson))
            {
                ModelState.AddModelError(string.Empty, "No hay datos válidos para importar. Primero debe subir y validar un archivo.");
                return View("CargaMasiva", model);
            }

            var dtos = JsonSerializer.Deserialize<List<UsuarioImportDto>>(model.DatosValidosJson)
                       ?? new List<UsuarioImportDto>();

            int creados = 0;
            int fallidos = 0;

            var resultadosFinales = new List<ResultadoFilaCargaMasiva>();

            foreach (var dto in dtos)
            {
                var (usuario, error) = await CrearUsuarioDesdeDtoAsync(dto);

                if (usuario != null)
                {
                    creados++;
                    await RegistrarAuditoriaCreacion(usuario);
                    resultadosFinales.Add(new ResultadoFilaCargaMasiva
                    {
                        NumeroLinea = dto.NumeroLinea,
                        TipoUsuario = dto.TipoUsuario,
                        Nombres = dto.Nombres,
                        Apellidos = dto.Apellidos,
                        DocumentoCI = dto.DocumentoCI,
                        Correo = dto.Correo,
                        EsValido = true,
                        Mensaje = "Usuario creado correctamente."
                    });
                }
                else
                {
                    fallidos++;
                    resultadosFinales.Add(new ResultadoFilaCargaMasiva
                    {
                        NumeroLinea = dto.NumeroLinea,
                        TipoUsuario = dto.TipoUsuario,
                        Nombres = dto.Nombres,
                        Apellidos = dto.Apellidos,
                        DocumentoCI = dto.DocumentoCI,
                        Correo = dto.Correo,
                        EsValido = false,
                        Mensaje = error ?? "Error desconocido al crear el usuario."
                    });
                }
            }

            var resumen = new CargaMasivaUsuariosViewModel
            {
                Resultados = resultadosFinales,
                TotalRegistros = dtos.Count,
                RegistrosValidos = creados,
                RegistrosInvalidos = fallidos,
                UsuariosCreados = creados,
                UsuariosFallidos = fallidos
            };

            return View("CargaMasiva", resumen);
        }

        private async Task<(Usuario? usuario, string? error)> CrearUsuarioDesdeDtoAsync(UsuarioImportDto dto)
        {
            var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == dto.TipoUsuario);
            if (rol == null)
                return (null, $"El rol '{dto.TipoUsuario}' no existe en el sistema.");

            if (await _context.Usuarios.AnyAsync(u => u.DocumentoCI == dto.DocumentoCI))
                return (null, $"El CI '{dto.DocumentoCI}' ya está registrado en la base de datos.");

            if (await _context.Usuarios.AnyAsync(u => u.Correo == dto.Correo))
                return (null, $"El correo '{dto.Correo}' ya está registrado en la base de datos.");

            if (dto.TipoUsuario == "Docente" && !string.IsNullOrWhiteSpace(dto.ItemDocente))
            {
                var existeItem = await _context.Usuarios.AnyAsync(u =>
                    u.TipoUsuario == "Docente" &&
                    u.ItemDocente == dto.ItemDocente);

                if (existeItem)
                    return (null, $"El ítem/código '{dto.ItemDocente}' ya está registrado para otro docente.");
            }

            bool esEstudiante = dto.TipoUsuario == "Estudiante";
            bool esPadre = dto.TipoUsuario == "Padre";
            bool esDocente = dto.TipoUsuario == "Docente";

            int? cursoId = null;
            bool pendienteAsignacionCurso = false;

            if (esEstudiante)
            {
                if (!dto.CursoId.HasValue)
                    return (null, "CursoId es obligatorio para Estudiante (carga masiva).");

                var cicloVigente = _configuration["PerfilCiclos:CicloVigente"];
                var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == dto.CursoId.Value);

                if (curso == null)
                    return (null, $"CursoId '{dto.CursoId.Value}' no corresponde a un curso válido.");

                if (!string.Equals(curso.Gestion, cicloVigente, StringComparison.OrdinalIgnoreCase))
                    return (null, $"El curso '{curso.Id}' no pertenece a la gestión vigente '{cicloVigente}'.");

                cursoId = curso.Id;
                pendienteAsignacionCurso = false;
            }

            var hijos = new List<Usuario>();
            bool pendienteVinculoEstudiantes = false;

            if (esPadre)
            {
                if (!string.IsNullOrWhiteSpace(dto.HijosCI))
                {
                    var cis = dto.HijosCI.Split('|', StringSplitOptions.RemoveEmptyEntries)
                                         .Select(ci => ci.Trim())
                                         .Where(ci => !string.IsNullOrEmpty(ci))
                                         .ToList();

                    if (cis.Any())
                    {
                        hijos = await _context.Usuarios
                            .Where(u => u.TipoUsuario == "Estudiante" && cis.Contains(u.DocumentoCI))
                            .ToListAsync();

                        if (hijos.Count != cis.Count)
                        {
                            var encontrados = hijos.Select(h => h.DocumentoCI).ToHashSet();
                            var faltantes = cis.Where(ci => !encontrados.Contains(ci));
                            return (null, $"No se encontraron todos los estudiantes para los CI: {string.Join(", ", faltantes)}.");
                        }
                    }
                }

                pendienteVinculoEstudiantes = !hijos.Any();
            }

            string? itemDocente = esDocente ? dto.ItemDocente : null;
            var asignaturas = new List<Asignatura>();
            bool pendienteAsignarAsignaturas = false;

            if (esDocente)
            {
                if (string.IsNullOrWhiteSpace(dto.ItemDocente))
                    return (null, "ItemDocente es obligatorio para Docente (carga masiva).");

                if (dto.AsignaturasIds.Any())
                {
                    asignaturas = await _context.Asignaturas
                        .Where(a => dto.AsignaturasIds.Contains(a.Id))
                        .ToListAsync();

                    if (asignaturas.Count != dto.AsignaturasIds.Count)
                    {
                        var encontrados = asignaturas.Select(a => a.Id).ToHashSet();
                        var faltantes = dto.AsignaturasIds.Where(id => !encontrados.Contains(id));
                        return (null, $"Algunas asignaturas no existen: IDs {string.Join(", ", faltantes)}.");
                    }

                    pendienteAsignarAsignaturas = false;
                }
                else
                {
                    pendienteAsignarAsignaturas = true;
                }
            }

            if (dto.FechaNacimiento == null)
                return (null, "FechaNacimiento no es válida.");

            var estadoCuentaInicial = rol.RequiereAprobacion ? "Pendiente" : "Habilitado";

            var usuario = new Usuario
            {
                Nombres = dto.Nombres,
                Apellidos = dto.Apellidos,
                DocumentoCI = dto.DocumentoCI,
                FechaNacimiento = dto.FechaNacimiento.Value,
                Correo = dto.Correo,
                Telefono = dto.Telefono,
                TipoUsuario = dto.TipoUsuario,
                EstadoCuenta = estadoCuentaInicial,
                RolId = rol.Id,
                CursoId = cursoId,
                PendienteAsignacionCurso = pendienteAsignacionCurso,
                PendienteVinculoEstudiantes = pendienteVinculoEstudiantes,
                ItemDocente = itemDocente,
                PendienteAsignarAsignaturas = pendienteAsignarAsignaturas
            };

            var (credencial, _) = await _servicioGestionUsuarios.GenerarCredencialesInicialesAsync(usuario);
            usuario.CredencialAcceso = credencial;

            if (rol.RequiereAprobacion)
            {
                var solicitud = _servicioGestionUsuarios.CrearSolicitudAprobacionRol(usuario, rol);
                _context.SolicitudesAprobacionRoles.Add(solicitud);
            }

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            if (esPadre && hijos.Any())
            {
                var vinculos = hijos.Select(h => new VinculoPadreEstudiante
                {
                    PadreId = usuario.Id,
                    EstudianteId = h.Id,
                    Relacion = "padre",
                    EsTutorLegal = true
                });

                _context.VinculosPadreEstudiante.AddRange(vinculos);
                usuario.PendienteVinculoEstudiantes = false;
                await _context.SaveChangesAsync();
            }

            if (esDocente && asignaturas.Any())
            {
                foreach (var asig in asignaturas)
                    usuario.Asignaturas.Add(asig);

                usuario.PendienteAsignarAsignaturas = false;
                await _context.SaveChangesAsync();
            }

            return (usuario, null);
        }
    }
    public class UsuarioImportDto
    {
        public int NumeroLinea { get; set; }

        public string TipoUsuario { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string DocumentoCI { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;

        public int? CursoId { get; set; }
        public string? HijosCI { get; set; }
        public string? ItemDocente { get; set; }
        public List<int> AsignaturasIds { get; set; } = new();
    }

}
