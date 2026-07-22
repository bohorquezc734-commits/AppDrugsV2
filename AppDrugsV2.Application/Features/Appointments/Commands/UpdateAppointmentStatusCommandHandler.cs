using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Domain.Entities;
using AppDrugsV2.Domain.Enums;

namespace AppDrugsV2.Application.Features.Appointments.Commands
{
    public class UpdateAppointmentStatusCommandHandler : IRequestHandler<UpdateAppointmentStatusCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public UpdateAppointmentStatusCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<bool>> Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
        {
            // Validar autenticación
            if (!_currentUserService.IsAuthenticated)
                return Result<bool>.Failure(AppConstants.Messages.UserNotAuthenticated);

            // Buscar el turno
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId && a.IsActive, cancellationToken);

            if (appointment == null)
                return Result<bool>.Failure($"El turno con ID {request.AppointmentId} {AppConstants.Messages.NotExistsKeyword}.");

            try
            {
                string notificationMessage;
                NotificationType notificationType;

                // Cambiar el estado según la transición
                switch (request.NewStatus)
                {
                    case AppointmentStatus.Recibido:
                        // Permite volver a Recibido (reversión)
                        notificationMessage = string.Format(AppConstants.NotificationMessages.AppointmentRecibido, appointment.Id);
                        notificationType = NotificationType.Info;
                        break;
                    case AppointmentStatus.EnProceso:
                        appointment.Confirmar();
                        notificationMessage = string.Format(AppConstants.NotificationMessages.AppointmentEnProceso, appointment.Id);
                        notificationType = NotificationType.Info;
                        break;
                    case AppointmentStatus.Entregado:
                        appointment.Entregar();
                        notificationMessage = string.Format(AppConstants.NotificationMessages.AppointmentEntregado, appointment.Id);
                        notificationType = NotificationType.Success;
                        break;
                    case AppointmentStatus.Cancelado:
                        appointment.Cancelar(request.Observaciones ?? AppConstants.Messages.DefaultCancellationReason);
                        notificationMessage = string.Format(
                            AppConstants.NotificationMessages.AppointmentCancelado,
                            appointment.Id,
                            request.Observaciones ?? AppConstants.Messages.DefaultCancellationReason);
                        notificationType = NotificationType.Warning;
                        break;
                    default:
                        return Result<bool>.Failure(AppConstants.Messages.InvalidAppointmentStatus);
                }

                // Crear notificación para el dueño del turno
                var notification = new Notification(appointment.UserId, notificationMessage, notificationType);
                await _context.Notifications.AddAsync(notification, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
            catch (InvalidOperationException ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
        }
    }
}
