using MediatR;

namespace AppDrugsV2.Domain.Events
{
    /// <summary>
    /// Contrato base para todos los eventos de dominio.
    /// Implementa INotification de MediatR para poder ser
    /// publicados y escuchados sin acoplamiento.
    /// </summary>
    public interface IDomainEvent : INotification
    {
        /// <summary>Identificador único del evento.</summary>
        Guid EventId { get; }

        /// <summary>Momento UTC en que ocurrió el evento.</summary>
        DateTime OccurredOn { get; }
    }
}
