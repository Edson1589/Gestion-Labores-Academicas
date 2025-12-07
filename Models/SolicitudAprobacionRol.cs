namespace GestionLaboresAcademicas.Models
{
    public class SolicitudAprobacionRol
    {
        public int Id { get; set; }

        public int UsuarioSolicitadoId { get; set; }
        public Usuario UsuarioSolicitado { get; set; } = null!;

        public int RolSolicitadoId { get; set; }
        public Rol RolSolicitado { get; set; } = null!;

        public string Estado { get; set; } = "Pendiente";

        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaRespuesta { get; set; }

        public string? MotivoRechazo { get; set; }
    }
}
