using MediatR;
using AppDrugsV2.Application.Features.Reports.DTOs;

namespace AppDrugsV2.Application.Features.Reports.Queries
{
    public class GetAppointmentsReportQuery : IRequest<List<AppointmentReportDto>>
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        /// <summary>
        /// Filtrar por estado: "Recibido", "EnProceso", "Entregado", "Cancelado"
        /// </summary>
        public string? Status { get; set; }
        public int? GestorFarmaceuticoId { get; set; }
    }
}
