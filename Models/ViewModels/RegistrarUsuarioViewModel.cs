using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GestionLaboresAcademicas.Models.ViewModels
{
    public class RegistrarUsuarioViewModel
    {
        [Required]
        [Display(Name = "Tipo de usuario")]
        public string TipoUsuario { get; set; } = null!;

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
        public List<SelectListItem> TiposUsuario { get; set; } = new();
        public List<SelectListItem> Cursos { get; set; } = new();

        [Display(Name = "Curso / paralelo")]
        public int? CursoId { get; set; }

        [Display(Name = "Guardar estudiante como pendiente de asignación de curso")]
        public bool GuardarComoPendienteCurso { get; set; }
        [Display(Name = "Buscar estudiante por nombre o CI")]
        public string? BuscarNombreOCI { get; set; }

        [Display(Name = "Curso del estudiante")]
        public int? BuscarCursoId { get; set; }

        public List<EstudianteSeleccionViewModel> EstudiantesDisponibles { get; set; } = new();

        [Display(Name = "Guardar padre sin vínculo (pendiente de vincular)")]
        public bool GuardarPadreSinVinculo { get; set; }
        [Display(Name = "Ítem / código interno")]
        public string? ItemDocente { get; set; }

        [Display(Name = "Asignaturas (opcional)")]
        public List<int> AsignaturasSeleccionadas { get; set; } = new();

        public List<SelectListItem> Asignaturas { get; set; } = new();
    }
}
