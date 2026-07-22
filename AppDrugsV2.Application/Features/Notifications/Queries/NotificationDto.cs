using AppDrugsV2.Domain.Enums;

namespace AppDrugsV2.Application.Features.Notifications.Queries
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
