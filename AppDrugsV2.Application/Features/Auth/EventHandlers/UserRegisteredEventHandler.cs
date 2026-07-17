using MediatR;
using Microsoft.Extensions.Logging;
using AppDrugsV2.Domain.Events;

namespace AppDrugsV2.Application.Features.Auth.EventHandlers
{
    /// <summary>
    /// Reacciona cuando un nuevo usuario se registra.
    /// Aquí puedes: enviar email de bienvenida, registrar auditoría, etc.
    /// </summary>
    public sealed class UserRegisteredEventHandler
        : INotificationHandler<UserRegisteredEvent>
    {
        private readonly ILogger<UserRegisteredEventHandler> _logger;

        public UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[Evento] Usuario registrado | Id: {UserId} | Email: {Email} | Rol: {Role} | En: {OccurredOn}",
                notification.UserId,
                notification.Email,
                notification.Role,
                notification.OccurredOn);

            // TODO: enviar email de bienvenida, notificación push, etc.

            return Task.CompletedTask;
        }
    }
}
