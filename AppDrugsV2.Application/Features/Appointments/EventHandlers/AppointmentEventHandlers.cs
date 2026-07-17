using MediatR;
using Microsoft.Extensions.Logging;
using AppDrugsV2.Domain.Events;

namespace AppDrugsV2.Application.Features.Appointments.EventHandlers
{
    /// <summary>
    /// Reacciona cuando se crea un nuevo turno.
    /// </summary>
    public sealed class AppointmentCreatedEventHandler
        : INotificationHandler<AppointmentCreatedEvent>
    {
        private readonly ILogger<AppointmentCreatedEventHandler> _logger;

        public AppointmentCreatedEventHandler(ILogger<AppointmentCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(AppointmentCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[Evento] Turno creado | Id: {AppointmentId} | Usuario: {UserId} | Gestor: {GestorId} | En: {OccurredOn}",
                notification.AppointmentId,
                notification.UserId,
                notification.GestorFarmaceuticoId,
                notification.OccurredOn);

            // TODO: notificar al gestor farmacéutico, enviar confirmación al usuario, etc.

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Reacciona cuando un turno es entregado.
    /// </summary>
    public sealed class AppointmentDeliveredEventHandler
        : INotificationHandler<AppointmentDeliveredEvent>
    {
        private readonly ILogger<AppointmentDeliveredEventHandler> _logger;

        public AppointmentDeliveredEventHandler(ILogger<AppointmentDeliveredEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(AppointmentDeliveredEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[Evento] Turno entregado | Id: {AppointmentId} | Usuario: {UserId} | En: {DeliveredAt}",
                notification.AppointmentId,
                notification.UserId,
                notification.DeliveredAt);

            // TODO: marcar completado, registrar métricas, encuesta de satisfacción, etc.

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Reacciona cuando un turno es cancelado.
    /// </summary>
    public sealed class AppointmentCancelledEventHandler
        : INotificationHandler<AppointmentCancelledEvent>
    {
        private readonly ILogger<AppointmentCancelledEventHandler> _logger;

        public AppointmentCancelledEventHandler(ILogger<AppointmentCancelledEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(AppointmentCancelledEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogWarning(
                "[Evento] Turno cancelado | Id: {AppointmentId} | Usuario: {UserId} | Motivo: {Observaciones}",
                notification.AppointmentId,
                notification.UserId,
                notification.Observaciones);

            // TODO: liberar slot de gestor, notificar al usuario, etc.

            return Task.CompletedTask;
        }
    }
}
