using GestionLaboresAcademicas.Data;
using GestionLaboresAcademicas.Models;
using GestionLaboresAcademicas.Models.DatosAcademicos;
using GestionLaboresAcademicas.Models.Estadisticas;
using GestionLaboresAcademicas.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace GestionLaboresAcademicas.Services
{
    public class ServicioEstadisticas
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ServicioEstadisticas>? _logger;

        private const decimal NotaAprobacion = 51m;

        public ServicioEstadisticas(
            ApplicationDbContext context,
            ILogger<ServicioEstadisticas>? logger = null)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ReporteEstadistico> ConsultarAsync(
            FiltroEstadistico filtro,
            Usuario usuarioActual,
            CancellationToken cancellationToken = default)
        {
            var reporte = new ReporteEstadistico
            {
                Filtros = filtro,
                Formato = "Vista",
                FechaGeneracion = DateTime.UtcNow,
                NombreInstitucion = "Institución educativa demo",
                NombreUsuario = $"{usuarioActual.Nombres} {usuarioActual.Apellidos}",
                RolUsuario = usuarioActual.Rol.Nombre
            };

            bool exito = true;
            string? mensajeError = null;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var tipos = filtro.TiposIndicador.ToHashSet();
                var rol = usuarioActual.Rol.Nombre;


                if (rol == "Estudiante")
                {
                    if (usuarioActual.CursoId.HasValue)
                    {
                        filtro.CursoId = usuarioActual.CursoId;
                    }
                }
                else if (rol == "Padre")
                {
                    var cursosHijosIds = await _context.VinculosPadreEstudiante
                        .Include(v => v.Estudiante)
                        .Where(v => v.PadreId == usuarioActual.Id && v.Estudiante.CursoId != null)
                        .Select(v => v.Estudiante.CursoId!.Value)
                        .Distinct()
                        .ToListAsync(cancellationToken);

                    if (cursosHijosIds.Any())
                    {
                        if (filtro.CursoId.HasValue && !cursosHijosIds.Contains(filtro.CursoId.Value))
                        {
                            throw new InvalidOperationException("No tiene permiso para consultar estadísticas de ese curso.");
                        }

                        if (!filtro.CursoId.HasValue && cursosHijosIds.Count == 1)
                        {
                            filtro.CursoId = cursosHijosIds[0];
                        }
                    }
                }

                List<Calificacion>? calificaciones = null;
                List<Asistencia>? asistencias = null;
                List<Matricula>? matriculas = null;

                if (tipos.Contains(TipoIndicador.PromocionReprobacion) ||
                    tipos.Contains(TipoIndicador.Promedios) ||
                    tipos.Contains(TipoIndicador.RendimientoPorMateria) ||
                    tipos.Contains(TipoIndicador.RendimientoPorCurso) ||
                    tipos.Contains(TipoIndicador.RendimientoPorDocente))
                {
                    var q = _context.Calificaciones
                        .Include(c => c.Curso)
                        .Include(c => c.Asignatura)
                        .Include(c => c.Estudiante)
                        .Include(c => c.PeriodoAcademico)
                        .AsNoTracking()
                        .AsQueryable();

                    if (filtro.PeriodoAcademicoId.HasValue)
                        q = q.Where(c => c.PeriodoAcademicoId == filtro.PeriodoAcademicoId.Value);

                    if (filtro.CursoId.HasValue)
                        q = q.Where(c => c.CursoId == filtro.CursoId.Value);

                    if (filtro.AsignaturaId.HasValue)
                        q = q.Where(c => c.AsignaturaId == filtro.AsignaturaId.Value);

                    if (filtro.RangoFechas != null)
                    {
                        if (filtro.RangoFechas.FechaInicio.HasValue)
                            q = q.Where(c => c.Fecha >= filtro.RangoFechas.FechaInicio.Value);

                        if (filtro.RangoFechas.FechaFin.HasValue)
                            q = q.Where(c => c.Fecha <= filtro.RangoFechas.FechaFin.Value);
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    calificaciones = await q.ToListAsync(cancellationToken);
                }

                if (tipos.Contains(TipoIndicador.Asistencia) ||
                    tipos.Contains(TipoIndicador.Desercion) ||
                    usuarioActual.Rol.Nombre == "Regente")
                {
                    var q = _context.Asistencias
                        .Include(a => a.Curso)
                        .Include(a => a.Asignatura)
                        .Include(a => a.Estudiante)
                        .Include(a => a.PeriodoAcademico)
                        .AsNoTracking()
                        .AsQueryable();

                    if (filtro.PeriodoAcademicoId.HasValue)
                        q = q.Where(a => a.PeriodoAcademicoId == filtro.PeriodoAcademicoId.Value);

                    if (filtro.CursoId.HasValue)
                        q = q.Where(a => a.CursoId == filtro.CursoId.Value);

                    if (filtro.AsignaturaId.HasValue)
                        q = q.Where(a => a.AsignaturaId == filtro.AsignaturaId.Value);

                    if (filtro.RangoFechas != null)
                    {
                        if (filtro.RangoFechas.FechaInicio.HasValue)
                            q = q.Where(a => a.Fecha >= filtro.RangoFechas.FechaInicio.Value);

                        if (filtro.RangoFechas.FechaFin.HasValue)
                            q = q.Where(a => a.Fecha <= filtro.RangoFechas.FechaFin.Value);
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    asistencias = await q.ToListAsync(cancellationToken);
                }

                if (tipos.Contains(TipoIndicador.Desercion))
                {
                    var q = _context.Matriculas
                        .Include(m => m.Curso)
                        .Include(m => m.PeriodoAcademico)
                        .Include(m => m.Estudiante)
                        .AsNoTracking()
                        .AsQueryable();

                    if (filtro.PeriodoAcademicoId.HasValue)
                        q = q.Where(m => m.PeriodoAcademicoId == filtro.PeriodoAcademicoId.Value);

                    if (filtro.CursoId.HasValue)
                        q = q.Where(m => m.CursoId == filtro.CursoId.Value);

                    if (filtro.RangoFechas != null)
                    {
                        if (filtro.RangoFechas.FechaInicio.HasValue)
                            q = q.Where(m => m.FechaInscripcion >= filtro.RangoFechas.FechaInicio.Value);

                        if (filtro.RangoFechas.FechaFin.HasValue)
                            q = q.Where(m => m.FechaInscripcion <= filtro.RangoFechas.FechaFin.Value);
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    matriculas = await q.ToListAsync(cancellationToken);
                }

                var esEstudianteOPadre = rol == "Estudiante" || rol == "Padre";

                if (calificaciones != null && calificaciones.Any() &&
                    (tipos.Contains(TipoIndicador.PromocionReprobacion) ||
                     tipos.Contains(TipoIndicador.Promedios) ||
                     tipos.Contains(TipoIndicador.RendimientoPorMateria) ||
                     tipos.Contains(TipoIndicador.RendimientoPorCurso)))
                {
                    var notasPorEstudiante = calificaciones
                        .GroupBy(c => new { c.CursoId, c.AsignaturaId, c.EstudianteId })
                        .Select(g => new
                        {
                            g.Key.CursoId,
                            g.Key.AsignaturaId,
                            g.Key.EstudianteId,
                            NotaFinal = g.Average(x => x.Nota)
                        })
                        .ToList();

                    var resumenCursoMateria = notasPorEstudiante
                        .GroupBy(x => new { x.CursoId, x.AsignaturaId })
                        .Select(g => new
                        {
                            g.Key.CursoId,
                            g.Key.AsignaturaId,
                            TotalEstudiantes = g.Count(),
                            Aprobados = g.Count(x => x.NotaFinal >= NotaAprobacion),
                            PromedioMateria = g.Average(x => x.NotaFinal)
                        })
                        .ToList();

                    decimal? promedioGeneral = notasPorEstudiante.Any()
                        ? notasPorEstudiante.Average(x => x.NotaFinal)
                        : null;

                    foreach (var item in resumenCursoMateria)
                    {
                        if (esEstudianteOPadre && item.TotalEstudiantes < 3)
                            continue;

                        var calRef = calificaciones.First(c =>
                            c.CursoId == item.CursoId &&
                            c.AsignaturaId == item.AsignaturaId);

                        var cursoNombre = calRef.Curso.NombreDisplay;
                        var materiaNombre = calRef.Asignatura.Nombre;

                        var porcentajeAprobacion = item.TotalEstudiantes == 0
                            ? 0
                            : (decimal)item.Aprobados / item.TotalEstudiantes * 100m;

                        var porcentajeReprobacion = item.TotalEstudiantes == 0
                            ? 0
                            : 100m - porcentajeAprobacion;

                        if (tipos.Contains(TipoIndicador.PromocionReprobacion))
                        {
                            reporte.Indicadores.Add(new IndicadorAcademico
                            {
                                Nombre = "% Aprobación",
                                Tipo = TipoIndicador.PromocionReprobacion,
                                Valor = decimal.Round(porcentajeAprobacion, 2),
                                Unidad = "%",
                                ClaveAgrupacion1 = cursoNombre,
                                ClaveAgrupacion2 = materiaNombre
                            });

                            reporte.Indicadores.Add(new IndicadorAcademico
                            {
                                Nombre = "% Reprobación",
                                Tipo = TipoIndicador.PromocionReprobacion,
                                Valor = decimal.Round(porcentajeReprobacion, 2),
                                Unidad = "%",
                                ClaveAgrupacion1 = cursoNombre,
                                ClaveAgrupacion2 = materiaNombre
                            });
                        }

                        if (tipos.Contains(TipoIndicador.Promedios))
                        {
                            reporte.Indicadores.Add(new IndicadorAcademico
                            {
                                Nombre = "Promedio por materia",
                                Tipo = TipoIndicador.Promedios,
                                Valor = decimal.Round(item.PromedioMateria, 2),
                                Unidad = "puntos",
                                ClaveAgrupacion1 = cursoNombre,
                                ClaveAgrupacion2 = materiaNombre
                            });
                        }
                    }

                    if (tipos.Contains(TipoIndicador.Promedios) && promedioGeneral.HasValue)
                    {
                        reporte.Indicadores.Add(new IndicadorAcademico
                        {
                            Nombre = "Promedio general",
                            Tipo = TipoIndicador.Promedios,
                            Valor = decimal.Round(promedioGeneral.Value, 2),
                            Unidad = "puntos",
                            ClaveAgrupacion1 = "General",
                            ClaveAgrupacion2 = null
                        });
                    }
                }

                if (asistencias != null && asistencias.Any())
                {
                    if (tipos.Contains(TipoIndicador.Asistencia))
                    {
                        var resumenAsistencia = asistencias
                            .GroupBy(a => new { a.CursoId, a.AsignaturaId })
                            .Select(g => new
                            {
                                g.Key.CursoId,
                                g.Key.AsignaturaId,
                                TotalRegistros = g.Count(),
                                Presentes = g.Count(x => x.Estado == "Presente")
                            })
                            .ToList();

                        foreach (var item in resumenAsistencia)
                        {
                            if (item.TotalRegistros == 0)
                                continue;

                            var aRef = asistencias.First(a =>
                                a.CursoId == item.CursoId &&
                                a.AsignaturaId == item.AsignaturaId);

                            var cursoNombre = aRef.Curso.NombreDisplay;
                            var materiaNombre = aRef.Asignatura.Nombre;

                            var porcentajeAsistencia = (decimal)item.Presentes / item.TotalRegistros * 100m;

                            reporte.Indicadores.Add(new IndicadorAcademico
                            {
                                Nombre = "% Asistencia",
                                Tipo = TipoIndicador.Asistencia,
                                Valor = decimal.Round(porcentajeAsistencia, 2),
                                Unidad = "%",
                                ClaveAgrupacion1 = cursoNombre,
                                ClaveAgrupacion2 = materiaNombre
                            });
                        }
                    }

                    if (usuarioActual.Rol.Nombre == "Regente")
                    {
                        DateTime fechaInicio, fechaFin;

                        if (filtro.RangoFechas != null &&
                            filtro.RangoFechas.FechaInicio.HasValue &&
                            filtro.RangoFechas.FechaFin.HasValue)
                        {
                            fechaInicio = filtro.RangoFechas.FechaInicio.Value.Date;
                            fechaFin = filtro.RangoFechas.FechaFin.Value.Date;
                        }
                        else
                        {
                            var periodo = asistencias.First().PeriodoAcademico;
                            fechaInicio = periodo.FechaInicio.Date;
                            fechaFin = periodo.FechaFin.Date;
                        }

                        var totalDiasCalendario = (fechaFin - fechaInicio).TotalDays + 1;
                        if (totalDiasCalendario < 1) totalDiasCalendario = 1;

                        var resumenRegente = asistencias
                            .GroupBy(a => a.CursoId)
                            .Select(g => new
                            {
                                CursoId = g.Key,
                                Curso = g.First().Curso,
                                TotalRegistros = g.Count(),
                                Presentes = g.Count(x => x.Estado == "Presente"),
                                Sanciones = g.Count(x => x.Estado != "Presente"),
                                DiasConClases = g.Select(x => x.Fecha.Date).Distinct().Count()
                            })
                            .ToList();

                        foreach (var item in resumenRegente)
                        {
                            if (item.TotalRegistros == 0)
                                continue;

                            var cursoNombre = item.Curso.NombreDisplay;

                            var porcentajeAsistenciaPromedio =
                                (decimal)item.Presentes / item.TotalRegistros * 100m;

                            var porcentajeCargaHoraria =
                                (decimal)item.DiasConClases / (decimal)totalDiasCalendario * 100m;

                            reporte.Indicadores.Add(new IndicadorAcademico
                            {
                                Nombre = "% Asistencia promedio (Regente)",
                                Tipo = TipoIndicador.Asistencia,
                                Valor = decimal.Round(porcentajeAsistenciaPromedio, 2),
                                Unidad = "%",
                                ClaveAgrupacion1 = cursoNombre,
                                ClaveAgrupacion2 = item.Curso.Turno
                            });

                            reporte.Indicadores.Add(new IndicadorAcademico
                            {
                                Nombre = "Sanciones registradas",
                                Tipo = TipoIndicador.RendimientoPorCurso,
                                Valor = item.Sanciones,
                                Unidad = "registros",
                                ClaveAgrupacion1 = cursoNombre,
                                ClaveAgrupacion2 = item.Curso.Turno
                            });
                            reporte.Indicadores.Add(new IndicadorAcademico
                            {
                                Nombre = "% Carga horaria cumplida",
                                Tipo = TipoIndicador.RendimientoPorCurso,
                                Valor = decimal.Round(porcentajeCargaHoraria, 2),
                                Unidad = "%",
                                ClaveAgrupacion1 = cursoNombre,
                                ClaveAgrupacion2 = item.Curso.Turno
                            });
                        }
                    }
                }

                if (matriculas != null && matriculas.Any() &&
                    tipos.Contains(TipoIndicador.Desercion))
                {
                    var resumenDesercion = matriculas
                        .GroupBy(m => m.CursoId)
                        .Select(g => new
                        {
                            CursoId = g.Key,
                            Curso = g.First().Curso,
                            Total = g
                                .Select(x => x.EstudianteId)
                                .Distinct()
                                .Count(),
                            Desertores = g
                                .Where(x => x.Estado == "Retirado")
                                .Select(x => x.EstudianteId)
                                .Distinct()
                                .Count()
                        })
                        .ToList();

                    foreach (var item in resumenDesercion)
                    {
                        if (item.Total == 0)
                            continue;

                        var porcentajeDesercion =
                            (decimal)item.Desertores / item.Total * 100m;

                        if ((usuarioActual.Rol.Nombre == "Estudiante" ||
                             usuarioActual.Rol.Nombre == "Padre") &&
                            item.Total < 3)
                        {
                            continue;
                        }

                        reporte.Indicadores.Add(new IndicadorAcademico
                        {
                            Nombre = "% Deserción",
                            Tipo = TipoIndicador.Desercion,
                            Valor = decimal.Round(porcentajeDesercion, 2),
                            Unidad = "%",
                            ClaveAgrupacion1 = item.Curso.NombreDisplay,
                            ClaveAgrupacion2 = null
                        });
                    }
                }
            }
            catch (OperationCanceledException)
            {
                exito = false;
                mensajeError = "Consulta cancelada por el usuario.";
                _logger?.LogWarning("Consulta de estadísticas cancelada por el usuario.");
                throw;
            }
            catch (Exception ex)
            {
                exito = false;
                mensajeError = ex.Message;
                _logger?.LogError(ex, "Error al ejecutar la consulta de estadísticas.");
                throw;
            }
            finally
            {
                await RegistrarBitacoraAsync(filtro, usuarioActual, "Consultar", exito, mensajeError, cancellationToken);
            }

            return reporte;
        }

        private async Task RegistrarBitacoraAsync(
            FiltroEstadistico filtro,
            Usuario usuario,
            string accion,
            bool exito,
            string? mensajeError,
            CancellationToken cancellationToken)
        {
            try
            {
                var serializableFiltro = new
                {
                    filtro.PeriodoAcademicoId,
                    filtro.CursoId,
                    filtro.AsignaturaId,
                    filtro.DocenteId,
                    filtro.Nivel,
                    filtro.Paralelo,
                    filtro.Turno,
                    FechaInicio = filtro.RangoFechas?.FechaInicio,
                    FechaFin = filtro.RangoFechas?.FechaFin
                };

                var filtrosJson = JsonSerializer.Serialize(serializableFiltro);

                var tiposIndicador = string.Join(
                    ",",
                    filtro.TiposIndicador.Select(t => t.ToString()));

                var bitacora = new BitacoraConsulta
                {
                    UsuarioId = usuario.Id,
                    FechaHora = DateTime.UtcNow,
                    Accion = accion,
                    Rol = usuario.Rol.Nombre,
                    FiltrosJson = filtrosJson,
                    TiposIndicador = tiposIndicador,
                    Exito = exito,
                    MensajeError = mensajeError
                };

                _context.BitacorasConsulta.Add(bitacora);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al registrar la bitácora de consulta de estadísticas.");
            }
        }

        internal async Task ConsultarAsync(ClaimsPrincipal user, EstadisticasFiltroViewModel model)
        {
            throw new NotImplementedException();
        }
    }
}
