namespace AppDrugsV2.Domain.Events
{
    /// <summary>
    /// Implementación base para todos los eventos de dominio.
    /// Provee EventId y OccurredOn automáticamente.
    /// </summary>
    public abstract record DomainEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
