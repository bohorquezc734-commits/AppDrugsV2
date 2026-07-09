using MediatR;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Domain.Enums;

namespace AppDrugsV2.Application.Features.Appointments.Commands
{
    public class UpdateAppointmentStatusCommand : IRequest<Result<bool>>
    {
        public int AppointmentId { get; set; }
        public AppointmentStatus NewStatus { get; set; }
        public string? Observaciones { get; set; }
    }
}
