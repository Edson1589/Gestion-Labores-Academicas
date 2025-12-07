namespace GestionLaboresAcademicas.Models.DatosAcademicos
{
    public class Matricula : DatoAcademico
    {
        public DateTime FechaInscripcion { get; set; }

        public string Estado { get; set; } = null!;
    }
}
