namespace GestionLaboresAcademicas.Models.DatosAcademicos
{
    public class Asistencia : DatoAcademico
    {
        public DateTime Fecha { get; set; }

        public string Estado { get; set; } = null!;
    }
}
