namespace GestionLaboresAcademicas.Models
{
    public class CredencialAcceso
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        public bool EsTemporal { get; set; } = true;
        public DateTime? FechaExpiracion { get; set; }
        public bool RequiereCambioPrimerLogin { get; set; } = true;

        public int IntentosFallidos { get; set; }
        public DateTime? BloqueadaHasta { get; set; }
    }
}
