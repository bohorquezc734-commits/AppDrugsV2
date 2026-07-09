using AppDrugsV2.Application.Common.Interfaces;

namespace AppDrugsV2.Infrastructure.Services.Auth
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password es requerido", nameof(password));

            // Usar BCrypt.Net.BCrypt
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string password, string hash)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password es requerido", nameof(password));

            if (string.IsNullOrEmpty(hash))
                throw new ArgumentException("Hash es requerido", nameof(hash));

            // Usar BCrypt.Net.BCrypt
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}