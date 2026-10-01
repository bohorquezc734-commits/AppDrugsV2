using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Domain.Entities;

namespace AppDrugsV2.Application.Features.Appointments.Commands
{
    public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CreateAppointmentCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<int>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
        {
       
            if (!_currentUserService.IsAuthenticated)
                return Result<int>.Failure(AppConstants.Messages.UserNotAuthenticated);

            var userId = _currentUserService.UserId!.Value;

            
            var gestor = await _context.GestoresFarmaceuticos
                .FirstOrDefaultAsync(g => g.Id == request.GestorFarmaceuticoId && g.IsActive, cancellationToken);
            if (gestor == null)
                return Result<int>.Failure(string.Format(AppConstants.Messages.SedeNotExists, request.GestorFarmaceuticoId));

            
            var appointment = new Appointment(
                userId,
                request.GestorFarmaceuticoId,
                request.ArchivoAutorizacion,
                request.ArchivoNombre,
                request.ArchivoContentType
            );

            
            await _context.Appointments.AddAsync(appointment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

         
            if (request.Details != null && request.Details.Any())
            {
                foreach (var detail in request.Details)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.Id == detail.InventoryId && i.IsActive, cancellationToken);

                    if (inventory == null)
                        return Result<int>.Failure(string.Format(AppConstants.Messages.InventoryNotExists, detail.InventoryId));

                    if (inventory.Quantity < detail.Quantity)
                        return Result<int>.Failure(string.Format(AppConstants.Messages.InsufficientStock, inventory.Quantity));

                    var appointmentDetail = new AppointmentDetail(
                        appointment.Id,  
                        detail.InventoryId,
                        detail.Quantity
                    );

                    inventory.RemoveStock(detail.Quantity);
                    appointment.AddDetail(appointmentDetail);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            
            var notification = new Notification(
                userId,
                string.Format(AppConstants.NotificationMessages.AppointmentCreado, appointment.Id, gestor.NombreSede),
                Domain.Enums.NotificationType.Success
            );
            await _context.Notifications.AddAsync(notification, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(appointment.Id);
        }
    }
}