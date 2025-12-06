namespace GestionLaboresAcademicas.Models
{
    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool RequiereAprobacion { get; set; }
        public string? AlcanceMaximo { get; set; }

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
