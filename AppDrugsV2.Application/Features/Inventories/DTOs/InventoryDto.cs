namespace AppDrugsV2.Application.Features.Inventories.DTOs
{
    public class InventoryDto
    {
        public int Id { get; set; }
        public int DrugId { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public int GestorFarmaceuticoId { get; set; }
        public string SedeName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}