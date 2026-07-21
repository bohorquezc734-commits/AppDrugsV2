using AppDrugsV2.Domain.Enums;

namespace AppDrugsV2.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string FullName { get; private set; }
        public UserRole Role { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public string? ResetPasswordToken { get; private set; }
        public DateTime? ResetPasswordTokenExpiresAt { get; private set; }

        // Constructor privado para EF Core (con valores por defecto)
        private User()
        {
            Email = string.Empty;
            PasswordHash = string.Empty;
            FullName = string.Empty;
        }

        public User(string email, string passwordHash, string fullName, UserRole role = UserRole.User)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email es requerido", nameof(email));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password es requerido", nameof(passwordHash));

            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Nombre completo es requerido", nameof(fullName));

            Email = email;
            PasswordHash = passwordHash;
            FullName = fullName;
            Role = role;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateProfile(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Nombre completo es requerido", nameof(fullName));

            FullName = fullName;
        }

        public void UpdatePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Password es requerido", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
        }

        public void RecordLogin() => LastLoginAt = DateTime.UtcNow;
        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

        public void SetResetPasswordToken(string token, DateTime expiresAt)
        {
            ResetPasswordToken = token;
            ResetPasswordTokenExpiresAt = expiresAt;
        }

        public void ClearResetPasswordToken()
        {
            ResetPasswordToken = null;
            ResetPasswordTokenExpiresAt = null;
        }
    }
}