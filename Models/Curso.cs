namespace GestionLaboresAcademicas.Models
{
    public class Curso
    {
        public int Id { get; set; }

        public string Nivel { get; set; } = null!;
        public string Grado { get; set; } = null!;
        public string Paralelo { get; set; } = null!;
        public string Turno { get; set; } = null!;
        public string Gestion { get; set; } = null!;

        public ICollection<Usuario> Estudiantes { get; set; } = new List<Usuario>();

        public string NombreDisplay =>
            $"{Nivel} {Grado} {Paralelo} - {Turno} ({Gestion})";
    }
}
