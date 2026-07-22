using Microsoft.AspNetCore.SignalR;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Infrastructure.Hubs;

namespace AppDrugsV2.Infrastructure.Services
{
    /// <summary>
    /// Implementación de <see cref="INotificationHubService"/> usando SignalR.
    /// Esta clase vive en Infrastructure → Application nunca la referencia directamente.
    /// </summary>
    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationHubService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <inheritdoc />
        public async Task SendToUserAsync(
            int               userId,
            string            eventName,
            object            payload,
            CancellationToken cancellationToken = default)
        {
            // Cada usuario tiene su grupo "user-{id}" (asignado en OnConnectedAsync).
            await _hubContext.Clients
                .Group($"user-{userId}")
                .SendAsync(eventName, payload, cancellationToken);
        }

        /// <inheritdoc />
        public async Task SendToAllAsync(
            string            eventName,
            object            payload,
            CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.All
                .SendAsync(eventName, payload, cancellationToken);
        }
    }
}
