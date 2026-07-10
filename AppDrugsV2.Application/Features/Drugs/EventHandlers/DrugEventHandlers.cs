using MediatR;
using Microsoft.Extensions.Logging;
using AppDrugsV2.Domain.Events;

namespace AppDrugsV2.Application.Features.Drugs.EventHandlers
{
    /// <summary>
    /// Reacciona cuando el stock de un medicamento llega a cero.
    /// </summary>
    public sealed class DrugOutOfStockEventHandler
        : INotificationHandler<DrugOutOfStockEvent>
    {
        private readonly ILogger<DrugOutOfStockEventHandler> _logger;

        public DrugOutOfStockEventHandler(ILogger<DrugOutOfStockEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(DrugOutOfStockEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogCritical(
                "[Evento] ¡STOCK AGOTADO! Medicamento: {DrugName} (Id: {DrugId}) | En: {OccurredOn}",
                notification.DrugName,
                notification.DrugId,
                notification.OccurredOn);

            // TODO: alertar a administrador, bloquear nuevos turnos, solicitar reposición, etc.

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Reacciona cuando el stock de un medicamento baja del umbral mínimo.
    /// </summary>
    public sealed class DrugLowStockEventHandler
        : INotificationHandler<DrugLowStockEvent>
    {
        private readonly ILogger<DrugLowStockEventHandler> _logger;

        public DrugLowStockEventHandler(ILogger<DrugLowStockEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(DrugLowStockEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogWarning(
                "[Evento] Stock bajo | Medicamento: {DrugName} (Id: {DrugId}) | Stock actual: {CurrentStock} | En: {OccurredOn}",
                notification.DrugName,
                notification.DrugId,
                notification.CurrentStock,
                notification.OccurredOn);

            // TODO: enviar alerta de reposición, crear orden de compra automática, etc.

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Reacciona cuando el stock de un medicamento cambia.
    /// </summary>
    public sealed class DrugStockChangedEventHandler
        : INotificationHandler<DrugStockChangedEvent>
    {
        private readonly ILogger<DrugStockChangedEventHandler> _logger;

        public DrugStockChangedEventHandler(ILogger<DrugStockChangedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(DrugStockChangedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[Evento] Stock modificado | {DrugName} (Id: {DrugId}) | {PreviousStock} → {NewStock} (delta: {Delta})",
                notification.DrugName,
                notification.DrugId,
                notification.PreviousStock,
                notification.NewStock,
                notification.Delta);

            return Task.CompletedTask;
        }
    }
}
