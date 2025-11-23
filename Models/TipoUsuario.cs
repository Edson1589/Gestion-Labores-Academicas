using System.ComponentModel.DataAnnotations;

namespace GestionLaboresAcademicas.Models
{
    public class TipoUsuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Tipo de usuario")]
        public string Nombre { get; set; } = string.Empty;
    }
}
