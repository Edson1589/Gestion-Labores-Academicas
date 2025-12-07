using System.Security.Cryptography;

namespace GestionLaboresAcademicas.Helpers
{
    public class PasswordHelper
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;
        private const char Delimiter = ';';

        public static string HashPassword(string password)
        {
            using var algorithm = new Rfc2898DeriveBytes(
                password,
                SaltSize,
                Iterations,
                HashAlgorithmName.SHA256);

            var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
            var salt = Convert.ToBase64String(algorithm.Salt);

            return $"{Iterations}{Delimiter}{salt}{Delimiter}{key}";
        }

        public static bool VerifyPassword(string password, string hash)
        {
            var parts = hash.Split(Delimiter);
            if (parts.Length != 3)
                return false;

            var iterations = Convert.ToInt32(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            using var algorithm = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256);

            var keyToCheck = algorithm.GetBytes(KeySize);
            return CryptographicOperations.FixedTimeEquals(key, keyToCheck);
        }
    }
}
