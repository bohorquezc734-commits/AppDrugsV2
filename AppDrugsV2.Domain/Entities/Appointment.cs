using AppDrugsV2.Domain.Enums;

namespace AppDrugsV2.Domain.Entities
{
    public class Appointment
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public int GestorFarmaceuticoId { get; private set; }
        public AppointmentStatus Status { get; private set; }
        public byte[]? ArchivoAutorizacion { get; private set; }
        public string? ArchivoNombre { get; private set; }
        public string? ArchivoContentType { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? FechaEntrega { get; private set; }
        public bool IsActive { get; private set; }
        public string? Observaciones { get; private set; }

        /// <summary>
        /// Código QR del turno en formato Base64 (generado por el caso de uso GenerateAppointmentQr).
        /// Nulo hasta que se genera por primera vez.
        /// </summary>
        public string? QrCodeBase64 { get; private set; }

        // Propiedades de navegación (para EF)
        public virtual User? User { get; private set; }
        public virtual GestorFarmaceutico? GestorFarmaceutico { get; private set; }
        public virtual ICollection<AppointmentDetail> Details { get; private set; } = new List<AppointmentDetail>();

        private Appointment() 
        {
            Details = new List<AppointmentDetail>();
        }

        public Appointment(int userId, int gestorFarmaceuticoId, byte[]? archivoAutorizacion = null, string? archivoNombre = null, string? archivoContentType = null)
        {
            if (userId <= 0)
                throw new ArgumentException("El ID del usuario es requerido.", nameof(userId));

            if (gestorFarmaceuticoId <= 0)
                throw new ArgumentException("El ID del gestor farmacéutico es requerido.", nameof(gestorFarmaceuticoId));

            UserId = userId;
            GestorFarmaceuticoId = gestorFarmaceuticoId;
            Status = AppointmentStatus.Recibido;
            ArchivoAutorizacion = archivoAutorizacion;
            ArchivoNombre = archivoNombre;
            ArchivoContentType = archivoContentType;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        // Métodos de comportamiento
        public void Confirmar()
        {
            if (Status != AppointmentStatus.Recibido)
                throw new InvalidOperationException("Solo se pueden confirmar turnos en estado 'Recibido'.");

            Status = AppointmentStatus.EnProceso;
        }

        public void Entregar()
        {
            if (Status != AppointmentStatus.EnProceso)
                throw new InvalidOperationException("Solo se pueden entregar turnos en estado 'En Proceso'.");

            Status = AppointmentStatus.Entregado;
            FechaEntrega = DateTime.UtcNow;
        }

        public void Cancelar(string observaciones)
        {
            if (Status == AppointmentStatus.Entregado)
                throw new InvalidOperationException("No se puede cancelar un turno ya entregado.");

            Status = AppointmentStatus.Cancelado;
            Observaciones = observaciones;
        }

        public void AddDetail(AppointmentDetail detail)
        {
            if (Status != AppointmentStatus.Recibido)
                throw new InvalidOperationException("Solo se pueden agregar detalles a turnos en estado 'Recibido'.");

            Details.Add(detail);
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Asigna el código QR (Base64) generado externamente.
        /// Solo el propio agregado puede mutar esta propiedad.
        /// </summary>
        public void AssignQrCode(string qrBase64)
        {
            if (string.IsNullOrWhiteSpace(qrBase64))
                throw new ArgumentException("El contenido del código QR no puede estar vacío.", nameof(qrBase64));

            QrCodeBase64 = qrBase64;
        }
    }
}