using AppDrugsV2.Domain.Enums;

namespace AppDrugsV2.Domain.Events
{
    // ─── USUARIO ──────────────────────────────────────────────────────────────────

    /// <summary>Se dispara cuando un nuevo usuario se registra en el sistema.</summary>
    public sealed record UserRegisteredEvent(
        int    UserId,
        string Email,
        string FullName,
        UserRole Role
    ) : DomainEvent;

    /// <summary>Se dispara cuando un usuario inicia sesión exitosamente.</summary>
    public sealed record UserLoggedInEvent(
        int    UserId,
        string Email
    ) : DomainEvent;

    // ─── TURNO / APPOINTMENT ──────────────────────────────────────────────────────

    /// <summary>Se dispara cuando un turno es creado.</summary>
    public sealed record AppointmentCreatedEvent(
        int AppointmentId,
        int UserId,
        int GestorFarmaceuticoId
    ) : DomainEvent;

    /// <summary>Se dispara cuando un turno pasa a estado "En Proceso".</summary>
    public sealed record AppointmentConfirmedEvent(
        int AppointmentId,
        int UserId,
        int GestorFarmaceuticoId
    ) : DomainEvent;

    /// <summary>Se dispara cuando un turno es entregado al paciente.</summary>
    public sealed record AppointmentDeliveredEvent(
        int      AppointmentId,
        int      UserId,
        DateTime DeliveredAt
    ) : DomainEvent;

    /// <summary>Se dispara cuando un turno es cancelado.</summary>
    public sealed record AppointmentCancelledEvent(
        int    AppointmentId,
        int    UserId,
        string Observaciones
    ) : DomainEvent;

    // ─── MEDICAMENTO / DRUG ────────────────────────────────────────────────────────

    /// <summary>Se dispara cuando se crea un nuevo medicamento.</summary>
    public sealed record DrugCreatedEvent(
        int     DrugId,
        string  Name,
        string  Category,
        decimal Price,
        int     Stock
    ) : DomainEvent;

    /// <summary>Se dispara cuando se actualiza la información de un medicamento.</summary>
    public sealed record DrugUpdatedEvent(
        int    DrugId,
        string Name,
        string Category
    ) : DomainEvent;

    /// <summary>Se dispara cuando el stock de un medicamento cambia.</summary>
    public sealed record DrugStockChangedEvent(
        int    DrugId,
        string DrugName,
        int    PreviousStock,
        int    NewStock,
        int    Delta
    ) : DomainEvent;

    /// <summary>Se dispara cuando el stock de un medicamento llega a cero.</summary>
    public sealed record DrugOutOfStockEvent(
        int    DrugId,
        string DrugName
    ) : DomainEvent;

    /// <summary>Se dispara cuando el stock baja del umbral mínimo configurado.</summary>
    public sealed record DrugLowStockEvent(
        int    DrugId,
        string DrugName,
        int    CurrentStock
    ) : DomainEvent;

    // ─── INVENTARIO ────────────────────────────────────────────────────────────────

    /// <summary>Se dispara cuando se registra un nuevo inventario.</summary>
    public sealed record InventoryCreatedEvent(
        int InventoryId,
        int DrugId,
        int GestorFarmaceuticoId,
        int InitialQuantity
    ) : DomainEvent;

    /// <summary>Se dispara cuando se agrega stock a un inventario.</summary>
    public sealed record InventoryStockAddedEvent(
        int InventoryId,
        int DrugId,
        int QuantityAdded,
        int NewTotal
    ) : DomainEvent;

    /// <summary>Se dispara cuando se quita stock de un inventario.</summary>
    public sealed record InventoryStockRemovedEvent(
        int InventoryId,
        int DrugId,
        int QuantityRemoved,
        int NewTotal
    ) : DomainEvent;
}
