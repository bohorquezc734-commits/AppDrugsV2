using MediatR;
using AppDrugsV2.Application.Features.Appointments.DTOs;
using AppDrugsV2.Domain.Enums;

namespace AppDrugsV2.Application.Features.Appointments.Queries
{
    public class ListAppointmentsQuery : IRequest<List<AppointmentDto>>
    {
        public int? UserId { get; set; }
        public int? GestorFarmaceuticoId { get; set; }
        public AppointmentStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}