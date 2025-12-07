namespace GestionLaboresAcademicas.Models
{
    public class RegistroAuditoriaUsuario
    {
        public int Id { get; set; }

        public int? UsuarioAfectadoId { get; set; }
        public Usuario? UsuarioAfectado { get; set; }

        public int? ActorId { get; set; }
        public Usuario? Actor { get; set; }

        public DateTime FechaHora { get; set; }

        public string Accion { get; set; } = null!;
        public string Detalle { get; set; } = null!;
        public string Origen { get; set; } = null!;
    }
}
