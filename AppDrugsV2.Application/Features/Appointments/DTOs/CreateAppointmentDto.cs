using AppDrugsV2.Domain.Enums;

namespace AppDrugsV2.Application.Features.Appointments.DTOs
{
    public class CreateAppointmentDto
    {
        public int GestorFarmaceuticoId { get; set; }
        public byte[]? ArchivoAutorizacion { get; set; }
        public string? ArchivoNombre { get; set; }
        public string? ArchivoContentType { get; set; }
        public List<CreateAppointmentDetailDto>? Details { get; set; }
    }

    public class CreateAppointmentDetailDto
    {
        public int InventoryId { get; set; }
        public int Quantity { get; set; }
    }
}