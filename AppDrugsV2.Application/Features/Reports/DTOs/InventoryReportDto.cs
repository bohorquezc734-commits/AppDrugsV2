namespace AppDrugsV2.Application.Features.Reports.DTOs
{
    public class InventoryReportDto
    {
        public int Id { get; set; }
        public string Medicamento { get; set; } = string.Empty;
        public string Sede { get; set; } = string.Empty;
        public int Stock { get; set; }
        public bool Activo { get; set; }
        public string EstadoTexto => Activo ? "Activo" : "Inactivo";
        public string EstadoStock => Stock switch
        {
            0 => "Sin stock",
            < 10 => "Stock bajo",
            _ => "Disponible"
        };
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
}
