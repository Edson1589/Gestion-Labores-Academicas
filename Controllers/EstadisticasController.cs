using GestionLaboresAcademicas.Data;
using GestionLaboresAcademicas.Models;
using GestionLaboresAcademicas.Models.Estadisticas;
using GestionLaboresAcademicas.Models.ViewModels;
using GestionLaboresAcademicas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using GestionLaboresAcademicas.Documents;
using QuestPDF.Fluent;

namespace GestionLaboresAcademicas.Controllers
{
    [Authorize]
    public class EstadisticasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ServicioEstadisticas _servicioEstadisticas;

        public EstadisticasController(ApplicationDbContext context, ServicioEstadisticas servicioEstadisticas)
        {
            _context = context;
            _servicioEstadisticas = servicioEstadisticas;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuario = await ObtenerUsuarioActualAsync();
            if (usuario == null)
                return RedirectToAction("Login", "Auth");

            var model = new EstadisticasFiltroViewModel();

            await RellenarMetadatosRolAsync(model, usuario);
            await RellenarCombosAsync(model, usuario);

            if (model.EsDocente && !model.Asignaturas.Any())
            {
                model.NoTieneMateriasDocente = true;
                model.MensajeInfo = "No tiene materias asociadas para el periodo seleccionado.";
            }

            ConfigurarVistaPorRol(model, usuario);

            AplicarOrdenYPaginacion(model);
            PrepararDatosGraficos(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(EstadisticasFiltroViewModel model, CancellationToken cancellationToken)
        {
            var usuario = await ObtenerUsuarioActualAsync();
            if (usuario == null)
                return RedirectToAction("Login", "Auth");

            await RellenarMetadatosRolAsync(model, usuario);
            await RellenarCombosAsync(model, usuario);

            ConfigurarVistaPorRol(model, usuario);
            var errores = new List<string>();

            var filtro = ConstruirFiltroDesdeViewModel(model, usuario, errores);

            await ValidarAlcanceUsuarioAsync(model, usuario, errores);

            foreach (var err in errores)
            {
                ModelState.AddModelError(string.Empty, err);
            }

            if (!ModelState.IsValid)
            {
                if (model.EsDocente && !model.Asignaturas.Any())
                {
                    model.NoTieneMateriasDocente = true;
                    model.MensajeInfo = "No tiene materias asociadas para el periodo seleccionado.";
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    model.Reporte = null;
                    model.TotalRegistros = 0;
                    model.IndicadoresPaginados = Enumerable.Empty<IndicadorAcademico>();
                    model.IndicadoresJson = "[]";

                    return PartialView("_ResultadosEstadisticas", model);
                }

                return View(model);
            }

            var reporte = await _servicioEstadisticas.ConsultarAsync(filtro, usuario, cancellationToken);
            model.Reporte = reporte;

            AplicarOrdenYPaginacion(model);
            PrepararDatosGraficos(model);

            if (model.TotalRegistros == 0 || reporte.Indicadores == null || !reporte.Indicadores.Any())
            {
                model.MensajeInfo = "Sin datos para los filtros seleccionados.";
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ResultadosEstadisticas", model);
            }

            return View(model);
        }

        private async Task<Usuario?> ObtenerUsuarioActualAsync()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var id))
                return null;

            return await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Curso)
                .Include(u => u.Asignaturas)
                .Include(u => u.VinculosComoPadre)
                    .ThenInclude(v => v.Estudiante)
                        .ThenInclude(e => e.Curso)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        private async Task RellenarMetadatosRolAsync(EstadisticasFiltroViewModel model, Usuario usuario)
        {
            var rol = usuario.Rol.Nombre;
            model.RolActual = rol;

            model.EsDocente = rol == "Docente";
            model.EsEstudianteOPadre = rol == "Estudiante" || rol == "Padre";

            model.PuedeVerVistaGlobal = rol == "Director" ||
                                        rol == "Secretaria" ||
                                        rol == "Regente" ||
                                        rol == "Bibliotecario";

            model.TiposIndicadorDisponibles = Enum
                .GetValues(typeof(TipoIndicador))
                .Cast<TipoIndicador>()
                .Select(t => new SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = t switch
                    {
                        TipoIndicador.PromocionReprobacion => "Promoción / Reprobación",
                        TipoIndicador.Promedios => "Promedios",
                        TipoIndicador.Asistencia => "Asistencia",
                        TipoIndicador.Desercion => "Deserción",
                        TipoIndicador.RendimientoPorMateria => "Rendimiento por materia",
                        TipoIndicador.RendimientoPorDocente => "Rendimiento por docente",
                        TipoIndicador.RendimientoPorCurso => "Rendimiento por curso",
                        _ => t.ToString()
                    }
                })
                .ToList();
        }

        private async Task RellenarCombosAsync(EstadisticasFiltroViewModel model, Usuario usuario)
        {
            model.Periodos = await _context.PeriodosAcademicos
                .OrderByDescending(p => p.FechaInicio)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Gestion} - {p.NombrePeriodo}"
                })
                .ToListAsync();

            var cursosQuery = _context.Cursos.AsQueryable();

            if (usuario.Rol.Nombre == "Estudiante" && usuario.CursoId.HasValue)
            {
                cursosQuery = cursosQuery.Where(c => c.Id == usuario.CursoId);
            }
            else if (usuario.Rol.Nombre == "Padre")
            {
                var cursosHijos = usuario.VinculosComoPadre
                    .Where(v => v.Estudiante.CursoId.HasValue)
                    .Select(v => v.Estudiante.CursoId!.Value)
                    .Distinct()
                    .ToList();

                if (cursosHijos.Any())
                {
                    cursosQuery = cursosQuery.Where(c => cursosHijos.Contains(c.Id));
                }
                else
                {
                    cursosQuery = cursosQuery.Where(c => false);
                }
            }

            model.Cursos = await cursosQuery
                .OrderBy(c => c.Nivel)
                .ThenBy(c => c.Grado)
                .ThenBy(c => c.Paralelo)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.NombreDisplay
                })
                .ToListAsync();

            if (usuario.Rol.Nombre == "Docente")
            {
                model.Asignaturas = usuario.Asignaturas
                    .OrderBy(a => a.Nombre)
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Nombre
                    })
                    .ToList();
            }
            else
            {
                model.Asignaturas = await _context.Asignaturas
                    .OrderBy(a => a.Nombre)
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Nombre
                    })
                    .ToListAsync();
            }

            if (model.PuedeVerVistaGlobal)
            {
                model.Docentes = await _context.Usuarios
                    .Where(u => u.Rol.Nombre == "Docente")
                    .OrderBy(u => u.Apellidos)
                    .ThenBy(u => u.Nombres)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = $"{u.Apellidos} {u.Nombres}"
                    })
                    .ToListAsync();
            }
        }

        private FiltroEstadistico ConstruirFiltroDesdeViewModel(
            EstadisticasFiltroViewModel model,
            Usuario usuario,
            List<string> errores)
        {
            var filtro = new FiltroEstadistico
            {
                PeriodoAcademicoId = model.PeriodoAcademicoId,
                CursoId = model.CursoId,
                AsignaturaId = model.AsignaturaId,
                DocenteId = model.DocenteId,
                Nivel = model.Nivel,
                Paralelo = model.Paralelo,
                Turno = model.Turno,
                RangoFechas = (model.FechaInicio.HasValue || model.FechaFin.HasValue)
                    ? new RangoFechas
                    {
                        FechaInicio = model.FechaInicio,
                        FechaFin = model.FechaFin
                    }
                    : null,
                TiposIndicador = model.TiposIndicadorSeleccionadosIds
                    .Select(id => (TipoIndicador)id)
                    .ToList()
            };

            var rol = usuario.Rol.Nombre;

            if (rol == "Docente")
            {
                filtro.DocenteId = usuario.Id;
                model.DocenteId = usuario.Id;
            }

            if (rol == "Estudiante")
            {
                if (!usuario.CursoId.HasValue)
                    errores.Add("No se encontró un curso asociado al estudiante.");
                else
                    filtro.CursoId = usuario.CursoId;
            }

            if (rol == "Padre")
            {
            }

            errores.AddRange(filtro.Validar());

            return filtro;
        }

        private async Task ValidarAlcanceUsuarioAsync(
            EstadisticasFiltroViewModel model,
            Usuario usuario,
            List<string> errores)
        {
            var rol = usuario.Rol.Nombre;

            if (rol == "Director" || rol == "Secretaria" || rol == "Regente" || rol == "Bibliotecario")
                return;

            if (rol == "Docente")
            {
                var asignaturasPermitidas = usuario.Asignaturas.Select(a => a.Id).ToHashSet();

                if (model.AsignaturaId.HasValue &&
                    !asignaturasPermitidas.Contains(model.AsignaturaId.Value))
                {
                    errores.Add("No puede consultar una materia que no está asignada a usted.");
                }

                return;
            }

            if (rol == "Estudiante")
            {
                if (model.CursoId.HasValue && usuario.CursoId.HasValue &&
                    model.CursoId.Value != usuario.CursoId.Value)
                {
                    errores.Add("Solo puede consultar estadísticas de su propio curso.");
                }
            }

            if (rol == "Padre")
            {
                var cursosHijos = usuario.VinculosComoPadre
                    .Where(v => v.Estudiante.CursoId.HasValue)
                    .Select(v => v.Estudiante.CursoId!.Value)
                    .Distinct()
                    .ToHashSet();

                if (model.CursoId.HasValue &&
                    !cursosHijos.Contains(model.CursoId.Value))
                {
                    errores.Add("Solo puede consultar estadísticas de los cursos de sus hijos.");
                }
            }
        }

        private void AplicarOrdenYPaginacion(EstadisticasFiltroViewModel model)
        {
            if (model.Reporte == null || model.Reporte.Indicadores == null)
            {
                model.IndicadoresPaginados = Enumerable.Empty<IndicadorAcademico>();
                model.TotalRegistros = 0;
                model.TotalesIndicadores = new List<TotalIndicadorViewModel>();
                return;
            }

            var query = model.Reporte.Indicadores.AsQueryable();

            switch (model.OrdenarPor)
            {
                case "Indicador":
                    query = model.DireccionOrden == "desc"
                        ? query.OrderByDescending(i => i.Nombre)
                        : query.OrderBy(i => i.Nombre);
                    break;

                case "Tipo":
                    query = model.DireccionOrden == "desc"
                        ? query.OrderByDescending(i => i.Tipo)
                        : query.OrderBy(i => i.Tipo);
                    break;

                case "Valor":
                    query = model.DireccionOrden == "desc"
                        ? query.OrderByDescending(i => i.Valor)
                        : query.OrderBy(i => i.Valor);
                    break;

                case "Grupo":
                default:
                    query = model.DireccionOrden == "desc"
                        ? query.OrderByDescending(i => i.ClaveAgrupacion1)
                               .ThenByDescending(i => i.ClaveAgrupacion2)
                        : query.OrderBy(i => i.ClaveAgrupacion1)
                               .ThenBy(i => i.ClaveAgrupacion2);
                    break;
            }

            var listaOrdenada = query.ToList();

            model.TotalRegistros = listaOrdenada.Count;

            model.TotalesIndicadores = listaOrdenada
                .GroupBy(i => new { i.Nombre, i.Tipo, i.Unidad })
                .Select(g =>
                {
                    var unidad = g.Key.Unidad ?? string.Empty;
                    decimal valorAgregado;

                    if (unidad == "%")
                    {
                        valorAgregado = g.Average(x => x.Valor);
                    }
                    else
                    {
                        valorAgregado = g.Sum(x => x.Valor);
                    }

                    return new TotalIndicadorViewModel
                    {
                        Nombre = g.Key.Nombre,
                        Tipo = g.Key.Tipo.ToString(),
                        Unidad = unidad,
                        Valor = decimal.Round(valorAgregado, 2)
                    };
                })
                .OrderBy(t => t.Tipo)
                .ThenBy(t => t.Nombre)
                .ToList();

            if (model.Pagina < 1) model.Pagina = 1;
            if (model.TamanoPagina <= 0) model.TamanoPagina = 20;

            var skip = (model.Pagina - 1) * model.TamanoPagina;

            model.IndicadoresPaginados = listaOrdenada
                .Skip(skip)
                .Take(model.TamanoPagina)
                .ToList();
        }

        private void PrepararDatosGraficos(EstadisticasFiltroViewModel model)
        {
            if (model.Reporte == null || model.Reporte.Indicadores == null)
            {
                model.IndicadoresJson = "[]";
                return;
            }

            var data = model.Reporte.Indicadores
                .Select(i => new
                {
                    i.Nombre,
                    Tipo = i.Tipo.ToString(),
                    i.Valor,
                    i.Unidad,
                    Grupo = i.ClaveAgrupacion1,
                    Detalle = i.ClaveAgrupacion2
                });

            model.IndicadoresJson = JsonSerializer.Serialize(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportarPdf(EstadisticasFiltroViewModel model)
        {
            var usuario = await ObtenerUsuarioActualAsync();
            if (usuario == null)
                return RedirectToAction("Login", "Auth");

            var errores = new List<string>();

            var filtro = ConstruirFiltroDesdeViewModel(model, usuario, errores);
            await ValidarAlcanceUsuarioAsync(model, usuario, errores);

            if (errores.Any())
            {
                TempData["Error"] = string.Join(" ", errores);
                return RedirectToAction(nameof(Index));
            }

            var reporte = await _servicioEstadisticas.ConsultarAsync(filtro, usuario);

            if (reporte == null || reporte.Indicadores == null || !reporte.Indicadores.Any())
            {
                TempData["MensajeInfo"] = "No hay datos para exportar con los filtros seleccionados.";
                return RedirectToAction(nameof(Index));
            }

            reporte.Formato = "PDF";

            var doc = new ReporteEstadisticoDocument(reporte);
            var pdfBytes = doc.GeneratePdf();

            var fileName = $"ReporteEstadistico_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }


        private void ConfigurarVistaPorRol(EstadisticasFiltroViewModel model, Usuario usuario)
        {
            var rol = usuario.Rol.Nombre;

            if (rol == "Estudiante")
            {
                model.EsVistaEstudianteOPadre = true;

                if (usuario.CursoId.HasValue)
                {
                    model.CursoId = usuario.CursoId;
                    model.DescripcionCursoActual = usuario.Curso?.NombreDisplay ?? "Curso asignado";
                }
            }
            else if (rol == "Padre")
            {
                model.EsVistaEstudianteOPadre = true;
                var cursos = usuario.VinculosComoPadre
                    .Where(v => v.Estudiante.Curso != null)
                    .Select(v => v.Estudiante.Curso!)
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .ToList();

                model.CursosHijos = cursos
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.NombreDisplay
                    })
                    .ToList();

                if (!model.CursoId.HasValue && cursos.Count == 1)
                {
                    model.CursoId = cursos[0].Id;
                }

                if (model.CursoId.HasValue)
                {
                    var cursoSel = cursos.FirstOrDefault(c => c.Id == model.CursoId.Value);
                    model.DescripcionCursoActual = cursoSel?.NombreDisplay;
                }
            }
        }

    }
}
