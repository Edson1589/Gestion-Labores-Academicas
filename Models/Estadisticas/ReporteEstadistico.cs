namespace GestionLaboresAcademicas.Models.Estadisticas
{
    public class ReporteEstadistico
    {
        public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;

        public string Formato { get; set; } = "Vista";

        public FiltroEstadistico Filtros { get; set; } = new();
        public List<IndicadorAcademico> Indicadores { get; set; } = new();

        public string? NombreInstitucion { get; set; }
        public string? NombreUsuario { get; set; }
        public string? RolUsuario { get; set; }
    }
}
