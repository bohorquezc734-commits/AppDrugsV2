using AppDrugsV2.Domain.Enums;

namespace AppDrugsV2.Domain.Entities
{
    public class Notification
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public NotificationType Type { get; private set; }
        public bool IsRead { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public User User { get; private set; } = null!;

        private Notification() { }

        public Notification(int userId, string message, NotificationType type = NotificationType.Info)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message is required", nameof(message));

            UserId = userId;
            Message = message;
            Type = type;
            IsRead = false;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}
