namespace AppDrugsV2.Domain.Entities
{
    public class Drug
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string GenericName { get; private set; }
        public string Laboratory { get; private set; }
        public decimal Price { get; private set; }
        public int Stock { get; private set; }
        public string Category { get; private set; }
        public bool RequiresPrescription { get; private set; }
        public DateTime ExpirationDate { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Constructor privado para EF Core
        private Drug()
        {
            Name = string.Empty;
            GenericName = string.Empty;
            Laboratory = string.Empty;
            Category = string.Empty;
        }

        public Drug(
            string name,
            string genericName,
            string laboratory,
            decimal price,
            int stock,
            string category,
            bool requiresPrescription,
            DateTime expirationDate)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nombre es requerido", nameof(name));

            if (string.IsNullOrWhiteSpace(genericName))
                throw new ArgumentException("Nombre genérico es requerido", nameof(genericName));

            if (string.IsNullOrWhiteSpace(laboratory))
                throw new ArgumentException("Laboratorio es requerido", nameof(laboratory));

            if (price <= 0)
                throw new ArgumentException("El precio debe ser mayor que 0", nameof(price));

            if (stock < 0)
                throw new ArgumentException("El stock no puede ser negativo", nameof(stock));

            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Categoría es requerida", nameof(category));

            if (expirationDate <= DateTime.UtcNow)
                throw new ArgumentException("La fecha de expiración debe ser futura", nameof(expirationDate));

            Name = name;
            GenericName = genericName;
            Laboratory = laboratory;
            Price = price;
            Stock = stock;
            Category = category;
            RequiresPrescription = requiresPrescription;
            ExpirationDate = expirationDate;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateStock(int quantity)
        {
            if (Stock + quantity < 0)
                throw new InvalidOperationException("Stock insuficiente");

            Stock += quantity;
        }

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice <= 0)
                throw new ArgumentException("El precio debe ser mayor que 0", nameof(newPrice));

            Price = newPrice;
        }

        public void UpdateInfo(string name, string genericName, string laboratory, string category, bool requiresPrescription, DateTime expirationDate)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nombre es requerido", nameof(name));

            if (string.IsNullOrWhiteSpace(genericName))
                throw new ArgumentException("Nombre genérico es requerido", nameof(genericName));

            if (string.IsNullOrWhiteSpace(laboratory))
                throw new ArgumentException("Laboratorio es requerido", nameof(laboratory));

            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Categoría es requerida", nameof(category));

            if (expirationDate <= DateTime.UtcNow)
                throw new ArgumentException("La fecha de expiración debe ser futura", nameof(expirationDate));

            Name = name;
            GenericName = genericName;
            Laboratory = laboratory;
            Category = category;
            RequiresPrescription = requiresPrescription;
            ExpirationDate = expirationDate;
        }

        public bool IsExpired() => DateTime.UtcNow > ExpirationDate;
        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}