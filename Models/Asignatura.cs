namespace GestionLaboresAcademicas.Models
{
    public class Asignatura
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Area { get; set; }
        public ICollection<Usuario> Docentes { get; set; } = new List<Usuario>();
    }
}
