using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Appointments.Commands
{

    public class GenerateAppointmentQrCommandHandler
        : IRequestHandler<GenerateAppointmentQrCommand, Result<string>>
    {
        private readonly IApplicationDbContext    _context;
        private readonly IQrCodeService           _qrCodeService;
        private readonly INotificationHubService  _notificationHub;

        public GenerateAppointmentQrCommandHandler(
            IApplicationDbContext   context,
            IQrCodeService          qrCodeService,
            INotificationHubService notificationHub)
        {
            _context         = context;
            _qrCodeService   = qrCodeService;
            _notificationHub = notificationHub;
        }

        public async Task<Result<string>> Handle(
            GenerateAppointmentQrCommand request,
            CancellationToken cancellationToken)
        {
     
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(
                    a => a.Id == request.AppointmentId && a.IsActive,
                    cancellationToken);

            if (appointment is null)
                return Result<string>.Failure(
                    $"El turno con ID {request.AppointmentId} {AppConstants.Messages.NotExistsKeyword}.");

          
            var qrContent  = $"APPDRUGS|TURNO:{appointment.Id}|USUARIO:{appointment.UserId}|FECHA:{appointment.CreatedAt:yyyy-MM-dd}";
            var qrBase64   = _qrCodeService.GenerateBase64(qrContent);

           
            try
            {
                appointment.AssignQrCode(qrBase64);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (ArgumentException ex)
            {
                return Result<string>.Failure(ex.Message);
            }

          
            try
            {
                await _notificationHub.SendToUserAsync(
                    userId:            appointment.UserId,
                    eventName:         "QrReady",
                    payload: new
                    {
                        appointmentId = appointment.Id,
                        message       = $"✅ El QR de tu turno #{appointment.Id} está listo.",
                        qrBase64
                    },
                    cancellationToken: cancellationToken);
            }
            catch
            {

            }

            return Result<string>.Success(qrBase64);
        }
    }
}
