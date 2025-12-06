using System.ComponentModel.DataAnnotations;

namespace GestionLaboresAcademicas.Models.ViewModels
{
    public class CambiarPasswordPrimerLoginViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string PasswordActual { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NuevaPassword { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("NuevaPassword", ErrorMessage = "La confirmación no coincide con la nueva contraseña.")]
        public string ConfirmarPassword { get; set; } = null!;
    }
}
