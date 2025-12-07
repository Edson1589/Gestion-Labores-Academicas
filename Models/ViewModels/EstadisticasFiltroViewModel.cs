using GestionLaboresAcademicas.Models.Estadisticas;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GestionLaboresAcademicas.Models.ViewModels
{
    public class EstadisticasFiltroViewModel
    {
        [Display(Name = "Periodo / Gestión")]
        public int? PeriodoAcademicoId { get; set; }

        [Display(Name = "Curso / Paralelo")]
        public int? CursoId { get; set; }

        [Display(Name = "Materia")]
        public int? AsignaturaId { get; set; }

        [Display(Name = "Docente")]
        public int? DocenteId { get; set; }

        [Display(Name = "Nivel")]
        public string? Nivel { get; set; }

        [Display(Name = "Paralelo")]
        public string? Paralelo { get; set; }

        [Display(Name = "Turno")]
        public string? Turno { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha inicio")]
        public DateTime? FechaInicio { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha fin")]
        public DateTime? FechaFin { get; set; }

        [Display(Name = "Tipos de indicador")]
        public List<int> TiposIndicadorSeleccionadosIds { get; set; } = new();


        public List<SelectListItem> Periodos { get; set; } = new();
        public List<SelectListItem> Cursos { get; set; } = new();
        public List<SelectListItem> Asignaturas { get; set; } = new();
        public List<SelectListItem> Docentes { get; set; } = new();
        public List<SelectListItem> TiposIndicadorDisponibles { get; set; } = new();


        public string RolActual { get; set; } = string.Empty;

        public bool PuedeVerVistaGlobal { get; set; }

        public bool EsDocente { get; set; }
        public bool NoTieneMateriasDocente { get; set; }

        public bool EsEstudianteOPadre { get; set; }

        public string? MensajeInfo { get; set; }

        public ReporteEstadistico? Reporte { get; set; }

        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 20;

        public int TotalRegistros { get; set; }

        public int TotalPaginas =>
            TotalRegistros <= 0 ? 1 : (int)Math.Ceiling((double)TotalRegistros / TamanoPagina);

        public string OrdenarPor { get; set; } = "Grupo";
        public string DireccionOrden { get; set; } = "asc";

        public IEnumerable<IndicadorAcademico> IndicadoresPaginados { get; set; }
            = Enumerable.Empty<IndicadorAcademico>();

        public string? IndicadoresJson { get; set; }

        public bool HayDatosGraficos =>
            Reporte != null && Reporte.Indicadores != null && Reporte.Indicadores.Any();

        public bool EsVistaEstudianteOPadre { get; set; }
        public string? DescripcionCursoActual { get; set; }

        public List<SelectListItem> CursosHijos { get; set; } = new();

        public bool TieneCursosHijos => CursosHijos.Any();

        public List<TotalIndicadorViewModel> TotalesIndicadores { get; set; } = new();
        public bool TieneTotales => TotalesIndicadores.Any();

    }
}
