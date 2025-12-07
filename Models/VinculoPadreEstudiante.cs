namespace GestionLaboresAcademicas.Models
{
    public class VinculoPadreEstudiante
    {
        public int Id { get; set; }

        public int PadreId { get; set; }
        public Usuario Padre { get; set; } = null!;

        public int EstudianteId { get; set; }
        public Usuario Estudiante { get; set; } = null!;

        public string Relacion { get; set; } = "padre";

        public bool EsTutorLegal { get; set; } = true;
    }
}
