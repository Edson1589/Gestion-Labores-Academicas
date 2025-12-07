namespace GestionLaboresAcademicas.Models.Estadisticas
{
    public class BitacoraConsulta
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public DateTime FechaHora { get; set; }

        public string Accion { get; set; } = null!;

        public string Rol { get; set; } = null!;

        public string FiltrosJson { get; set; } = null!;

        public string TiposIndicador { get; set; } = null!;

        public bool Exito { get; set; } = true;
        public string? MensajeError { get; set; }
    }
}
