namespace GestionLaboresAcademicas.Models.Estadisticas
{
    public class FiltroEstadistico
    {
        public int? PeriodoAcademicoId { get; set; }
        public int? CursoId { get; set; }
        public int? AsignaturaId { get; set; }
        public int? DocenteId { get; set; }
        public string? Nivel { get; set; }
        public string? Paralelo { get; set; }
        public string? Turno { get; set; }
        public RangoFechas? RangoFechas { get; set; }

        public List<TipoIndicador> TiposIndicador { get; set; } = new();

        public List<string> Validar()
        {
            var errores = new List<string>();

            if (!PeriodoAcademicoId.HasValue)
                errores.Add("El periodo/gestión es obligatorio.");

            if (RangoFechas != null &&
                RangoFechas.FechaInicio.HasValue &&
                RangoFechas.FechaFin.HasValue &&
                RangoFechas.FechaFin < RangoFechas.FechaInicio)
            {
                errores.Add("La fecha fin no puede ser anterior a la fecha inicio.");
            }

            if (!TiposIndicador.Any())
                errores.Add("Debe seleccionar al menos un tipo de indicador.");

            return errores;
        }
    }
}
