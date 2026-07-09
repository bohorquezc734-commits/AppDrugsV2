namespace AppDrugsV2.Application.Features.Reports.DTOs
{
    public class AppointmentReportDto
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Sede { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string? ArchivoAdjunto { get; set; }
        public string? Observaciones { get; set; }
        /// <summary>
        /// Lista de medicamentos en formato "NombreMedicamento (x Cantidad)"
        /// </summary>
        public List<string> Medicamentos { get; set; } = new();

        /// <summary>
        /// Representación textual de los medicamentos para el Excel (columna única)
        /// </summary>
        public string MedicamentosTexto => string.Join(", ", Medicamentos);
    }
}
