using GestionLaboresAcademicas.Data;
using GestionLaboresAcademicas.Helpers;
using GestionLaboresAcademicas.Models;
using GestionLaboresAcademicas.Models.ViewModels;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace GestionLaboresAcademicas.Services
{
    public class ServicioGestionUsuarios
    {
        private readonly ApplicationDbContext _context;

        public ServicioGestionUsuarios(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(CredencialAcceso credencial, string passwordTemporal)> GenerarCredencialesInicialesAsync(Usuario usuario)
        {
            var politica = await _context.PoliticasSeguridad.FirstOrDefaultAsync()
                          ?? new PoliticaSeguridad();

            var username = await GenerarUsernameUnicoAsync(usuario);

            var passwordTemporal = GenerarPasswordTemporal(politica);

            var passwordHash = PasswordHelper.HashPassword(passwordTemporal);

            var credencial = new CredencialAcceso
            {
                Username = username,
                PasswordHash = passwordHash,
                EsTemporal = true,
                RequiereCambioPrimerLogin = true,
                FechaExpiracion = DateTime.UtcNow.AddDays(30),
                IntentosFallidos = 0,
                BloqueadaHasta = null,
                Usuario = usuario
            };

            return (credencial, passwordTemporal);
        }

        public SolicitudAprobacionRol CrearSolicitudAprobacionRol(Usuario usuario, Rol rol)
        {
            return new SolicitudAprobacionRol
            {
                UsuarioSolicitado = usuario,
                RolSolicitadoId = rol.Id,
                RolSolicitado = rol,
                Estado = "Pendiente",
                FechaSolicitud = DateTime.UtcNow
            };
        }

        // ==========================================
        // MÉTODOS PARA ELIMINACIÓN DE USUARIOS (CU5)
        // ==========================================

        /// <summary>
        /// Obtiene todas las dependencias de un usuario para evaluar el impacto de su eliminación
        /// </summary>
        public async Task<DependenciasUsuarioDto> ObtenerDependenciasUsuarioAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.VinculosComoPadre).ThenInclude(v => v.Estudiante)
                .Include(u => u.VinculosComoEstudiante)
                .Include(u => u.Asignaturas)
                .Include(u => u.Curso)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
                throw new InvalidOperationException("Usuario no encontrado.");

            var dependencias = new DependenciasUsuarioDto
            {
                CantidadVinculosPadre = usuario.VinculosComoPadre.Count,
                CantidadVinculosEstudiante = usuario.VinculosComoEstudiante.Count,
                CantidadAsignaturas = usuario.Asignaturas.Count,

                EstudiantesVinculados = usuario.VinculosComoPadre
                    .Select(v => $"{v.Estudiante.Nombres} {v.Estudiante.Apellidos}")
                    .ToList(),

                AsignaturasAsignadas = usuario.Asignaturas
                    .Select(a => a.Nombre)
                    .ToList(),

                CursoAsignado = usuario.Curso != null
                    ? $"{usuario.Curso.Nivel} {usuario.Curso.Grado} {usuario.Curso.Paralelo}"
                    : null
            };

            // Contar datos académicos
            dependencias.CantidadCalificaciones = await _context.Calificaciones
                .CountAsync(c => c.EstudianteId == usuarioId);

            dependencias.CantidadAsistencias = await _context.Asistencias
                .CountAsync(a => a.EstudianteId == usuarioId);

            dependencias.CantidadMatriculas = await _context.Matriculas
                .CountAsync(m => m.EstudianteId == usuarioId);

            dependencias.CantidadPrestamos = await _context.PrestamosBiblioteca
                .CountAsync(p => p.EstudianteId == usuarioId);

            dependencias.CantidadSolicitudesRol = await _context.SolicitudesAprobacionRoles
                .CountAsync(s => s.UsuarioSolicitadoId == usuarioId);

            return dependencias;
        }

        /// <summary>
        /// Valida si el usuario puede ser eliminado
        /// </summary>
        public async Task<(bool puedeEliminar, string? mensajeError)> ValidarEliminacionUsuarioAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null)
                return (false, "Usuario no encontrado.");

            if (usuario.Eliminado)
                return (false, "El usuario ya ha sido eliminado previamente.");

            // Validar si tiene operaciones críticas en curso
            // Por ejemplo, préstamos activos de biblioteca
            var tienePrestamosActivos = await _context.PrestamosBiblioteca
                .AnyAsync(p => p.EstudianteId == usuarioId && p.Estado == "Activo");

            if (tienePrestamosActivos)
                return (false, "El usuario tiene préstamos de biblioteca activos. Debe devolverlos antes de eliminar.");

            // Validar si es el único administrador/director
            if (usuario.TipoUsuario == "Director" || usuario.TipoUsuario == "Administrador")
            {
                var cantidadAdmins = await _context.Usuarios
                    .CountAsync(u => u.TipoUsuario == usuario.TipoUsuario && !u.Eliminado && u.Id != usuarioId);

                if (cantidadAdmins == 0)
                    return (false, $"No se puede eliminar el último {usuario.TipoUsuario} del sistema.");
            }

            return (true, null);
        }

        /// <summary>
        /// Ejecuta la eliminación lógica del usuario
        /// </summary>
        public async Task<bool> EjecutarEliminacionUsuarioAsync(
            int usuarioId,
            int actorId,
            string motivo)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.CredencialAcceso)
                    .FirstOrDefaultAsync(u => u.Id == usuarioId);

                if (usuario == null)
                    return false;

                // Marcar como eliminado (soft delete)
                usuario.Eliminado = true;
                usuario.EliminadoEl = DateTime.UtcNow;
                usuario.EliminadoPor = actorId;
                usuario.MotivoEliminacion = motivo;

                // Cambiar estado de cuenta
                usuario.EstadoCuenta = "Eliminado";

                // Deshabilitar credenciales de acceso
                if (usuario.CredencialAcceso != null)
                {
                    usuario.CredencialAcceso.BloqueadaHasta = DateTime.MaxValue;
                }

                // Limpiar vínculos activos si es padre
                if (usuario.TipoUsuario == "Padre")
                {
                    var vinculos = await _context.VinculosPadreEstudiante
                        .Where(v => v.PadreId == usuarioId)
                        .ToListAsync();

                    _context.VinculosPadreEstudiante.RemoveRange(vinculos);
                }

                // Limpiar asignaturas si es docente
                if (usuario.TipoUsuario == "Docente")
                {
                    var asignaturas = await _context.Usuarios
                        .Where(u => u.Id == usuarioId)
                        .SelectMany(u => u.Asignaturas)
                        .ToListAsync();

                    usuario.Asignaturas.Clear();
                }

                // Desasignar curso si es estudiante
                if (usuario.TipoUsuario == "Estudiante")
                {
                    usuario.CursoId = null;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ==========================================
        // MÉTODOS PARA GESTIÓN DE ESTADOS (CU6)
        // ==========================================

        /// <summary>
        /// Valida si un usuario puede cambiar a un estado específico
        /// </summary>
        public async Task<(bool puedeTransicionar, string? mensajeError)> ValidarTransicionEstadoAsync(
            int usuarioId,
            string estadoDestino)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.CredencialAcceso)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
                return (false, "Usuario no encontrado.");

            var estadoActual = usuario.EstadoCuenta;

            // Validar transiciones permitidas
            var transicionesValidas = new Dictionary<string, List<string>>
            {
                { "Habilitado", new List<string> { "Desactivado", "Bloqueado" } },
                { "Pendiente", new List<string> { "Habilitado", "Desactivado", "Bloqueado" } },
                { "Desactivado", new List<string> { "Habilitado", "Bloqueado" } },
                { "Bloqueado", new List<string> { "Habilitado" } }
            };

            if (!transicionesValidas.ContainsKey(estadoActual))
                return (false, $"El estado actual '{estadoActual}' no es válido.");

            if (!transicionesValidas[estadoActual].Contains(estadoDestino))
                return (false, $"No se puede cambiar de '{estadoActual}' a '{estadoDestino}'.");

            // Validaciones específicas por estado destino
            if (estadoDestino == "Habilitado")
            {
                // Verificar que tenga credenciales válidas
                if (usuario.CredencialAcceso == null)
                    return (false, "El usuario no tiene credenciales de acceso configuradas.");

                // Verificar que no esté eliminado
                if (usuario.Eliminado)
                    return (false, "No se puede activar un usuario eliminado.");
            }

            return (true, null);
        }

        /// <summary>
        /// Activa un usuario desactivado o bloqueado
        /// </summary>
        public async Task<bool> ActivarUsuarioAsync(int usuarioId, int actorId, string motivo)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.CredencialAcceso)
                    .FirstOrDefaultAsync(u => u.Id == usuarioId);

                if (usuario == null)
                    return false;

                var estadoAnterior = usuario.EstadoCuenta;

                // Cambiar estado
                usuario.EstadoCuenta = "Habilitado";

                // Limpiar información de bloqueo
                usuario.BloqueadoHasta = null;
                usuario.TipoBloqueo = null;
                usuario.MotivoBloqueo = null;
                usuario.BloqueadoPor = null;

                // Limpiar información de desactivación
                usuario.DesactivadoEl = null;
                usuario.MotivoDesactivacion = null;
                usuario.DesactivadoPor = null;

                // Rehabilitar credenciales
                if (usuario.CredencialAcceso != null)
                {
                    usuario.CredencialAcceso.BloqueadaHasta = null;
                    usuario.CredencialAcceso.IntentosFallidos = 0;
                }

                await _context.SaveChangesAsync();

                // Registrar en historial
                await RegistrarCambioEstadoAsync(usuarioId, actorId, estadoAnterior, "Habilitado", motivo, "Manual");

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Desactiva un usuario activo
        /// </summary>
        public async Task<bool> DesactivarUsuarioAsync(int usuarioId, int actorId, string motivo)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.CredencialAcceso)
                    .FirstOrDefaultAsync(u => u.Id == usuarioId);

                if (usuario == null)
                    return false;

                var estadoAnterior = usuario.EstadoCuenta;

                // Cambiar estado
                usuario.EstadoCuenta = "Desactivado";
                usuario.DesactivadoEl = DateTime.UtcNow;
                usuario.MotivoDesactivacion = motivo;
                usuario.DesactivadoPor = actorId;

                // Invalidar sesiones (bloquear credenciales)
                if (usuario.CredencialAcceso != null)
                {
                    usuario.CredencialAcceso.BloqueadaHasta = DateTime.MaxValue;
                }

                await _context.SaveChangesAsync();

                // Registrar en historial
                await RegistrarCambioEstadoAsync(usuarioId, actorId, estadoAnterior, "Desactivado", motivo, "Manual");

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Bloquea un usuario de forma temporal o permanente
        /// </summary>
        public async Task<bool> BloquearUsuarioAsync(
            int usuarioId,
            int actorId,
            string motivo,
            string tipoBloqueo,
            int? duracionMinutos = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.CredencialAcceso)
                    .FirstOrDefaultAsync(u => u.Id == usuarioId);

                if (usuario == null)
                    return false;

                var estadoAnterior = usuario.EstadoCuenta;

                // Cambiar estado
                usuario.EstadoCuenta = "Bloqueado";
                usuario.TipoBloqueo = tipoBloqueo;
                usuario.MotivoBloqueo = motivo;
                usuario.BloqueadoPor = actorId;

                // Configurar duración del bloqueo
                if (tipoBloqueo == "Temporal" && duracionMinutos.HasValue)
                {
                    usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(duracionMinutos.Value);
                }
                else if (tipoBloqueo == "Permanente")
                {
                    usuario.BloqueadoHasta = DateTime.MaxValue;
                }

                // Invalidar sesiones inmediatamente
                if (usuario.CredencialAcceso != null)
                {
                    usuario.CredencialAcceso.BloqueadaHasta = usuario.BloqueadoHasta;
                }

                await _context.SaveChangesAsync();

                // Registrar en historial
                await RegistrarCambioEstadoAsync(usuarioId, actorId, estadoAnterior, "Bloqueado", motivo, "Manual");

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Bloquea automáticamente un usuario por exceso de intentos fallidos
        /// </summary>
        public async Task<bool> BloquearUsuarioAutomaticoAsync(int usuarioId, int numeroIntentos, string direccionIP)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.CredencialAcceso)
                    .FirstOrDefaultAsync(u => u.Id == usuarioId);

                if (usuario == null)
                    return false;

                var politica = await _context.PoliticasSeguridad.FirstOrDefaultAsync();
                var duracionBloqueoMinutos = politica?.MinutosBloqueo ?? 15;

                var estadoAnterior = usuario.EstadoCuenta;

                // Cambiar estado
                usuario.EstadoCuenta = "Bloqueado";
                usuario.TipoBloqueo = "Automatico";
                usuario.MotivoBloqueo = $"Bloqueo automático por {numeroIntentos} intentos fallidos desde IP: {direccionIP}";
                usuario.BloqueadoPor = null; // No hay actor humano
                usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(duracionBloqueoMinutos);

                // Bloquear credenciales
                if (usuario.CredencialAcceso != null)
                {
                    usuario.CredencialAcceso.BloqueadaHasta = usuario.BloqueadoHasta;
                }

                await _context.SaveChangesAsync();

                // Registrar en historial
                var motivo = $"Bloqueo automático: {numeroIntentos} intentos fallidos desde {direccionIP}. " +
                            $"Desbloqueado automáticamente el {usuario.BloqueadoHasta?.ToString("dd/MM/yyyy HH:mm")}";

                await RegistrarCambioEstadoAsync(usuarioId, null, estadoAnterior, "Bloqueado", motivo, "Automatico");

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Desbloquea automáticamente usuarios cuyo tiempo de bloqueo ha expirado
        /// </summary>
        public async Task<int> DesbloquearUsuariosAutomaticoAsync()
        {
            var usuariosBloqueados = await _context.Usuarios
                .Where(u => u.EstadoCuenta == "Bloqueado" &&
                            u.BloqueadoHasta.HasValue &&
                            u.BloqueadoHasta.Value <= DateTime.UtcNow)
                .ToListAsync();

            int desbloqueados = 0;

            foreach (var usuario in usuariosBloqueados)
            {
                var estadoAnterior = usuario.EstadoCuenta;

                usuario.EstadoCuenta = "Habilitado";
                usuario.BloqueadoHasta = null;
                usuario.TipoBloqueo = null;
                usuario.MotivoBloqueo = null;
                usuario.BloqueadoPor = null;

                await RegistrarCambioEstadoAsync(
                    usuario.Id,
                    null,
                    estadoAnterior,
                    "Habilitado",
                    "Desbloqueo automático por vencimiento del tiempo de bloqueo",
                    "Automatico");

                desbloqueados++;
            }

            if (desbloqueados > 0)
            {
                await _context.SaveChangesAsync();
            }

            return desbloqueados;
        }

        /// <summary>
        /// Registra un cambio de estado en el historial
        /// </summary>
        private async Task RegistrarCambioEstadoAsync(
            int usuarioId,
            int? actorId,
            string estadoAnterior,
            string estadoNuevo,
            string motivo,
            string tipoCambio)
        {
            var historial = new HistorialEstadoUsuario
            {
                UsuarioAfectadoId = usuarioId,
                ActorId = actorId,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = estadoNuevo,
                FechaHora = DateTime.UtcNow,
                Motivo = motivo,
                TipoCambio = tipoCambio,
                DireccionIP = "N/A" // Esto se puede mejorar pasándolo como parámetro
            };

            _context.HistorialEstadosUsuarios.Add(historial);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Obtiene el historial de cambios de estado de un usuario
        /// </summary>
        public async Task<List<HistorialEstadoUsuario>> ObtenerHistorialEstadosAsync(int usuarioId)
        {
            return await _context.HistorialEstadosUsuarios
                .Include(h => h.Actor)
                .Where(h => h.UsuarioAfectadoId == usuarioId)
                .OrderByDescending(h => h.FechaHora)
                .ToListAsync();
        }

        // ==========================================
        // MÉTODOS PRIVADOS AUXILIARES
        // ==========================================

        private async Task<string> GenerarUsernameUnicoAsync(Usuario usuario)
        {
            var nombres = (usuario.Nombres ?? "").Trim();
            var apellidos = (usuario.Apellidos ?? "").Trim();

            var inicial = !string.IsNullOrEmpty(nombres)
                ? nombres[0].ToString()
                : "u";

            var primerApellido = apellidos
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault() ?? "user";

            var baseUsername = (inicial + primerApellido).ToLowerInvariant();

            baseUsername = new string(baseUsername
                .Where(char.IsLetterOrDigit)
                .ToArray());

            if (string.IsNullOrEmpty(baseUsername))
                baseUsername = "user";

            string username = baseUsername;
            int sufijo = 1;

            while (await _context.CredencialesAcceso.AnyAsync(c => c.Username == username))
            {
                username = $"{baseUsername}{sufijo}";
                sufijo++;
            }

            return username;
        }

        private string GenerarPasswordTemporal(PoliticaSeguridad politica)
        {
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string special = "!@#$%&*?";

            var rng = RandomNumberGenerator.Create();
            var chars = new List<char>();

            if (politica.RequiereMinusculas)
                chars.Add(GetRandomChar(lower, rng));

            if (politica.RequiereMayusculas)
                chars.Add(GetRandomChar(upper, rng));

            if (politica.RequiereNumero)
                chars.Add(GetRandomChar(digits, rng));

            if (politica.RequiereCaracterEspecial)
                chars.Add(GetRandomChar(special, rng));

            var pool = new StringBuilder();
            if (politica.RequiereMinusculas) pool.Append(lower);
            if (politica.RequiereMayusculas) pool.Append(upper);
            if (politica.RequiereNumero) pool.Append(digits);
            if (politica.RequiereCaracterEspecial) pool.Append(special);

            if (pool.Length == 0)
                pool.Append(lower + upper + digits + special);

            while (chars.Count < politica.LongitudMinimaPassword)
            {
                chars.Add(GetRandomChar(pool.ToString(), rng));
            }

            for (int i = chars.Count - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars.ToArray());
        }

        private static char GetRandomChar(string source, RandomNumberGenerator rng)
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var value = BitConverter.ToUInt32(bytes, 0);
            var index = (int)(value % (uint)source.Length);
            return source[index];
        }
    }
}