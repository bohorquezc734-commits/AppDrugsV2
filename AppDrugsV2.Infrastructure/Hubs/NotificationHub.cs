using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AppDrugsV2.Infrastructure.Hubs
{
    /// <summary>
    /// Hub de SignalR para notificaciones en tiempo real.
    /// Los clientes se conectan a "/hubs/notifications".
    ///
    /// Convención de grupos: cada usuario autenticado se une
    /// al grupo "user-{userId}" al conectarse, de modo que
    /// el servidor puede enviarle mensajes sin conocer su ConnectionId.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Al conectarse, el usuario se une a su grupo personal.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userIdClaim = Context.User?.FindFirst("nameid")?.Value
                           ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim is not null)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userIdClaim}");

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Al desconectarse, el usuario abandona su grupo automáticamente
        /// (SignalR lo hace por nosotros, pero se puede agregar lógica extra aquí).
        /// </summary>
        public override Task OnDisconnectedAsync(Exception? exception)
            => base.OnDisconnectedAsync(exception);
    }
}
