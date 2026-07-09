namespace AppDrugsV2.Application.Features.Appointments.DTOs
{
    public class AppointmentDetailDto
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public int InventoryId { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}