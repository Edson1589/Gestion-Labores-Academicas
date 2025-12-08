using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionLaboresAcademicas.Models
{
    public class RegistroAuditoriaUsuario
    {
        [Key]
        public int Id { get; set; }

        // Usuario afectado por la acción
        public int? UsuarioAfectadoId { get; set; }
        [ForeignKey(nameof(UsuarioAfectadoId))]
        public Usuario? UsuarioAfectado { get; set; }

        // Usuario que realizó la acción (puede ser null para acciones automáticas)
        public int? ActorId { get; set; }
        [ForeignKey(nameof(ActorId))]
        public Usuario? Actor { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [Required]
        [MaxLength(50)]
        public string Accion { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Detalle { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DireccionIP { get; set; }

        [MaxLength(200)]
        public string Origen { get; set; } = string.Empty;
    }
}