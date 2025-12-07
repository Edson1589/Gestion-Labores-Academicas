using GestionLaboresAcademicas.Data;
using GestionLaboresAcademicas.Helpers;
using GestionLaboresAcademicas.Models;
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
