using GestionLaboresAcademicas.Models.DatosAcademicos;

namespace GestionLaboresAcademicas.Models
{
    public class PeriodoAcademico
    {
        public int Id { get; set; }

        public string Gestion { get; set; } = null!;

        public string NombrePeriodo { get; set; } = null!;

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public string Estado { get; set; } = "Activo";

        public ICollection<DatoAcademico> DatosAcademicos { get; set; }
            = new List<DatoAcademico>();
    }
}
