using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GestionLaboresAcademicas.Models.ViewModels
{
    public class EditarUsuarioViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Tipo de usuario")]
        public string TipoUsuario { get; set; } = null!;

        [Display(Name = "Estado de cuenta")]
        public string EstadoCuenta { get; set; } = null!;

        [Required]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; } = null!;

        [Required]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; } = null!;

        [Required]
        [Display(Name = "Documento/CI")]
        public string DocumentoCI { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Correo")]
        public string Correo { get; set; } = null!;

        [Required]
        [Phone]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = null!;

        [Display(Name = "Curso / paralelo")]
        public int? CursoId { get; set; }
        public bool PendienteAsignacionCurso { get; set; }
        public List<SelectListItem> Cursos { get; set; } = new();

        [Display(Name = "Ítem / código interno")]
        public string? ItemDocente { get; set; }
        public bool PendienteAsignarAsignaturas { get; set; }

        [Display(Name = "Asignaturas")]
        public List<int> AsignaturasSeleccionadas { get; set; } = new();
        public List<SelectListItem> Asignaturas { get; set; } = new();

        public bool EsEstudiante => TipoUsuario == "Estudiante";
        public bool EsDocente => TipoUsuario == "Docente";
    }
}
