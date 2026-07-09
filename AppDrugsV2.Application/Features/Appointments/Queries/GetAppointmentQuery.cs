using MediatR;
using AppDrugsV2.Application.Features.Appointments.DTOs;

namespace AppDrugsV2.Application.Features.Appointments.Queries
{
    public class GetAppointmentQuery : IRequest<AppointmentDto>
    {
        public int Id { get; set; }
    }
}