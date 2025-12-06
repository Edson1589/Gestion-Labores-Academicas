namespace GestionLaboresAcademicas.Models.ViewModels
{
    public class HistorialUsuarioViewModel
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string TipoUsuario { get; set; } = null!;

        public List<RegistroAuditoriaUsuario> Registros { get; set; } = new();
    }
}
