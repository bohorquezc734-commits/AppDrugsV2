using AppDrugsV2.Domain.Enums;

namespace AppDrugsV2.Application.Features.Appointments.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int GestorFarmaceuticoId { get; set; }
        public string SedeName { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string? ArchivoNombre { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string? Observaciones { get; set; }
        public bool IsActive { get; set; }
        public List<AppointmentDetailDto> Details { get; set; } = new();
    }
}