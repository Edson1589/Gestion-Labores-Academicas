namespace GestionLaboresAcademicas.Models.ViewModels
{
    public class EliminarUsuarioViewModel
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string DocumentoCI { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string EstadoCuenta { get; set; } = string.Empty;

        // Dependencias del usuario
        public DependenciasUsuarioDto Dependencias { get; set; } = new();

        // Confirmación
        public string TextoConfirmacion { get; set; } = string.Empty;
        public string MotivoEliminacion { get; set; } = string.Empty;
        public bool TieneOperacionesCriticas { get; set; } = false;
        public string MensajeOperacionesCriticas { get; set; } = string.Empty;
    }

    public class DependenciasUsuarioDto
    {
        public int CantidadVinculosPadre { get; set; }
        public int CantidadVinculosEstudiante { get; set; }
        public int CantidadAsignaturas { get; set; }
        public int CantidadCalificaciones { get; set; }
        public int CantidadAsistencias { get; set; }
        public int CantidadMatriculas { get; set; }
        public int CantidadPrestamos { get; set; }
        public int CantidadSolicitudesRol { get; set; }

        public List<string> EstudiantesVinculados { get; set; } = new();
        public List<string> AsignaturasAsignadas { get; set; } = new();
        public string? CursoAsignado { get; set; }

        public bool TieneDependencias =>
            CantidadVinculosPadre > 0 ||
            CantidadVinculosEstudiante > 0 ||
            CantidadAsignaturas > 0 ||
            CantidadCalificaciones > 0 ||
            CantidadAsistencias > 0 ||
            CantidadMatriculas > 0 ||
            CantidadPrestamos > 0 ||
            CantidadSolicitudesRol > 0;
    }
}