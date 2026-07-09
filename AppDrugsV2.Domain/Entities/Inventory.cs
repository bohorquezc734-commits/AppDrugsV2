using AppDrugsV2.Domain.Entities;

namespace AppDrugsV2.Domain.Entities
{
    public class Inventory
    {
        public int Id { get; private set; }
        public int DrugId { get; private set; }
        public int GestorFarmaceuticoId { get; private set; }
        public int Quantity { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Propiedades de navegación (para EF)
        public virtual Drug? Drug { get; private set; }
        public virtual GestorFarmaceutico? GestorFarmaceutico { get; private set; }

        private Inventory() { }

        public Inventory(int drugId, int gestorFarmaceuticoId, int quantity)
        {
            if (drugId <= 0)
                throw new ArgumentException("El ID del medicamento es requerido.", nameof(drugId));

            if (gestorFarmaceuticoId <= 0)
                throw new ArgumentException("El ID del gestor farmacéutico es requerido.", nameof(gestorFarmaceuticoId));

            if (quantity < 0)
                throw new ArgumentException("La cantidad no puede ser negativa.", nameof(quantity));

            DrugId = drugId;
            GestorFarmaceuticoId = gestorFarmaceuticoId;
            Quantity = quantity;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateStock(int newQuantity)
        {
            if (newQuantity < 0)
                throw new ArgumentException("La cantidad no puede ser negativa.", nameof(newQuantity));

            Quantity = newQuantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddStock(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("La cantidad a agregar no puede ser negativa.", nameof(quantity));

            Quantity += quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveStock(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("La cantidad a remover no puede ser negativa.", nameof(quantity));

            if (Quantity - quantity < 0)
                throw new InvalidOperationException("No hay suficiente stock disponible.");

            Quantity -= quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }
    }
}