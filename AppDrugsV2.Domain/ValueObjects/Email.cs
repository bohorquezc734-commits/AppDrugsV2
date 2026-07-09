namespace AppDrugsV2.Domain.ValueObjects
{
    public class Email
    {
        public string Value { get; private set; }

        private Email(string value)
        {
            Value = value ?? string.Empty;
        }

        public static Email Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email es requerido", nameof(value));

            if (!IsValidFormat(value))
                throw new ArgumentException("Formato de email inválido", nameof(value));

            return new Email(value.ToLowerInvariant().Trim());
        }

        private static bool IsValidFormat(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email.Trim();
            }
            catch
            {
                return false;
            }
        }

        public override string ToString() => Value ?? string.Empty;  // ← Arreglado

        public override bool Equals(object? obj)  // ← Arreglado (object? con ?)
        {
            return obj is Email other && Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;  // ← Arreglado
        }

        public static implicit operator string(Email email) => email?.Value ?? string.Empty;  // ← Arreglado
    }
}