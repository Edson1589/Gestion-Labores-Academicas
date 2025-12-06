using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace GestionLaboresAcademicas.Models.ViewModels
{
    public class RechazarSolicitudRolViewModel
    {
        public int Id { get; set; }

        [ValidateNever]
        public string? UsuarioNombre { get; set; }

        [ValidateNever]
        public string? RolNombre { get; set; }

        [Required(ErrorMessage = "El motivo del rechazo es obligatorio.")]
        [StringLength(500, ErrorMessage = "El motivo no puede superar los 500 caracteres.")]
        [Display(Name = "Motivo del rechazo")]
        public string Motivo { get; set; } = string.Empty;
    }
}
