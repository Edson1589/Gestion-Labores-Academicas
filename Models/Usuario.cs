namespace GestionLaboresAcademicas.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        public string Nombres { get; set; } = null!;
        public string Apellidos { get; set; } = null!;
        public string DocumentoCI { get; set; } = null!;
        public DateTime FechaNacimiento { get; set; }

        public string Correo { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string TipoUsuario { get; set; } = null!;

        public string EstadoCuenta { get; set; } = "Habilitado";

        public int RolId { get; set; }
        public Rol Rol { get; set; } = null!;
        public int? CursoId { get; set; }
        public Curso? Curso { get; set; }
        public bool PendienteAsignacionCurso { get; set; } = false;
        public CredencialAcceso? CredencialAcceso { get; set; }

        public ICollection<VinculoPadreEstudiante> VinculosComoPadre { get; set; }
            = new List<VinculoPadreEstudiante>();

        public ICollection<VinculoPadreEstudiante> VinculosComoEstudiante { get; set; }
            = new List<VinculoPadreEstudiante>();
        public bool PendienteVinculoEstudiantes { get; set; } = false;
        public string? ItemDocente { get; set; }

        public ICollection<Asignatura> Asignaturas { get; set; } = new List<Asignatura>();

        public bool PendienteAsignarAsignaturas { get; set; } = false;

        public ICollection<RegistroAuditoriaUsuario> AuditoriasComoAfectado { get; set; } = new List<RegistroAuditoriaUsuario>();
        public ICollection<RegistroAuditoriaUsuario> AuditoriasComoActor { get; set; } = new List<RegistroAuditoriaUsuario>();
        public ICollection<SolicitudAprobacionRol> SolicitudesRol { get; set; } = new List<SolicitudAprobacionRol>();

    }
}
