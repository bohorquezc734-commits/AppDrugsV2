namespace AppDrugsV2.Application.Features.Gestores.DTOs
{
    public class GestorDto
    {
        public int Id { get; set; }
        public string NombreSede { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public int IdEps { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}