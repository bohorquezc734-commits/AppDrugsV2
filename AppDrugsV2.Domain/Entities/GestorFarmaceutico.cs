using AppDrugsV2.Domain.Enums;

namespace AppDrugsV2.Domain.Entities
{
    public class GestorFarmaceutico
    {
        // Propiedades (solo lectura desde fuera)
        public int Id { get; private set; }
        public string? NombreSede { get; private set; }
        public string? Direccion { get; private set; }
        public string?  Telefono { get; private set; }
        public int IdEps { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Constructor privado para EF Core
        private GestorFarmaceutico() { }

        // Constructor público para crear nuevas sedes
        public GestorFarmaceutico(string nombreSede, string direccion, string telefono, int idEps)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(nombreSede))
                throw new ArgumentException("El nombre de la sede es requerido.", nameof(nombreSede));

            if (string.IsNullOrWhiteSpace(direccion))
                throw new ArgumentException("La dirección es requerida.", nameof(direccion));

            if (idEps <= 0)
                throw new ArgumentException("El ID de la EPS es requerido.", nameof(idEps));

            NombreSede = nombreSede;
            Direccion = direccion;
            Telefono = telefono ?? string.Empty;
            IdEps = idEps;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        // Métodos de comportamiento (la lógica de negocio)
        public void Update(string nombreSede, string direccion, string telefono, int idEps)
        {
            if (string.IsNullOrWhiteSpace(nombreSede))
                throw new ArgumentException("El nombre de la sede es requerido.", nameof(nombreSede));

            if (string.IsNullOrWhiteSpace(direccion))
                throw new ArgumentException("La dirección es requerida.", nameof(direccion));

            if (idEps <= 0)
                throw new ArgumentException("El ID de la EPS es requerido.", nameof(idEps));

            NombreSede = nombreSede;
            Direccion = direccion;
            Telefono = telefono ?? string.Empty;
            IdEps = idEps;
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