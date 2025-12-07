namespace GestionLaboresAcademicas.Models.DatosAcademicos
{
    public class PrestamoBiblioteca : DatoAcademico
    {
        public DateTime FechaPrestamo { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public string Estado { get; set; } = null!;
    }
}
