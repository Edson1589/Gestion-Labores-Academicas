namespace GestionLaboresAcademicas.Models
{
    public class HistorialEstadoUsuario
    {
        public int Id { get; set; }

        public int UsuarioAfectadoId { get; set; }
        public Usuario UsuarioAfectado { get; set; } = null!;

        public int? ActorId { get; set; }
        public Usuario? Actor { get; set; }

        public string EstadoAnterior { get; set; } = null!;
        public string EstadoNuevo { get; set; } = null!;

        public DateTime FechaHora { get; set; }

        public string Motivo { get; set; } = null!;
        public string TipoCambio { get; set; } = null!; // "Manual", "Automatico"

        public string? DireccionIP { get; set; }
    }
}