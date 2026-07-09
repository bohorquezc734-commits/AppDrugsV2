namespace AppDrugsV2.Domain.Entities
{
    public class AppointmentDetail
    {
        public int Id { get; private set; }
        public int AppointmentId { get; private set; }
        public int InventoryId { get; private set; }
        public int Quantity { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Propiedades de navegación
        public virtual Appointment? Appointment { get; private set; }
        public virtual Inventory? Inventory { get; private set; }

        private AppointmentDetail() { }

        public AppointmentDetail(int appointmentId, int inventoryId, int quantity)
        {
            if (appointmentId <= 0)
                throw new ArgumentException("El ID del turno es requerido.", nameof(appointmentId));

            if (inventoryId <= 0)
                throw new ArgumentException("El ID del inventario es requerido.", nameof(inventoryId));

            if (quantity <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.", nameof(quantity));

            AppointmentId = appointmentId;
            InventoryId = inventoryId;
            Quantity = quantity;
            CreatedAt = DateTime.UtcNow;
        }
    }
}