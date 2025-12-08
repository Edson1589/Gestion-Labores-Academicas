namespace GestionLaboresAcademicas.Models.ViewModels
{
    /// <summary>
    /// ViewModel para visualizar el detalle de un usuario antes de cambiar su estado
    /// </summary>
    public class DetalleEstadoUsuarioViewModel
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string DocumentoCI { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string EstadoCuentaActual { get; set; } = string.Empty;

        // Información de bloqueo actual
        public bool EstaBloqueado { get; set; }
        public DateTime? BloqueadoHasta { get; set; }
        public string? TipoBloqueo { get; set; }
        public string? MotivoBloqueo { get; set; }

        // Información de desactivación
        public bool EstaDesactivado { get; set; }
        public DateTime? DesactivadoEl { get; set; }
        public string? MotivoDesactivacion { get; set; }

        // Sesiones activas
        public List<SesionActivaDto> SesionesActivas { get; set; } = new();

        // Historial de cambios de estado
        public List<HistorialEstadoUsuario> HistorialEstados { get; set; } = new();

        // Transiciones permitidas
        public bool PuedeActivar { get; set; }
        public bool PuedeDesactivar { get; set; }
        public bool PuedeBloquear { get; set; }
        public bool PuedeDesbloquear { get; set; }
    }

    /// <summary>
    /// ViewModel para activar un usuario
    /// </summary>
    public class ActivarUsuarioViewModel
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = string.Empty;

        public string MotivoActivacion { get; set; } = string.Empty;

        // Requisitos pendientes
        public List<string> RequisitosPendientes { get; set; } = new();
        public bool CumpleRequisitos { get; set; } = true;
    }

    /// <summary>
    /// ViewModel para desactivar un usuario
    /// </summary>
    public class DesactivarUsuarioViewModel
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = string.Empty;

        public string MotivoDesactivacion { get; set; } = string.Empty;

        // Sesiones activas que serán cerradas
        public int CantidadSesionesActivas { get; set; }
        public List<SesionActivaDto> SesionesActivas { get; set; } = new();
    }

    /// <summary>
    /// ViewModel para bloquear un usuario
    /// </summary>
    public class BloquearUsuarioViewModel
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = string.Empty;

        public string TipoBloqueo { get; set; } = "Temporal"; // "Temporal" o "Permanente"
        public string MotivoBloqueo { get; set; } = string.Empty;

        // Para bloqueo temporal
        public int? DuracionMinutos { get; set; }
        public int? DuracionHoras { get; set; }
        public int? DuracionDias { get; set; }

        // Sesiones activas que serán cerradas
        public int CantidadSesionesActivas { get; set; }
        public List<SesionActivaDto> SesionesActivas { get; set; } = new();
    }

    /// <summary>
    /// DTO para representar sesiones activas
    /// </summary>
    public class SesionActivaDto
    {
        public string Dispositivo { get; set; } = string.Empty;
        public string Navegador { get; set; } = string.Empty;
        public string DireccionIP { get; set; } = string.Empty;
        public DateTime HoraInicio { get; set; }
        public DateTime UltimaActividad { get; set; }
    }

    /// <summary>
    /// ViewModel para visualizar el historial completo de cambios de estado
    /// </summary>
    public class HistorialEstadosUsuarioViewModel
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = string.Empty;

        public List<HistorialEstadoUsuario> Registros { get; set; } = new();
    }
}