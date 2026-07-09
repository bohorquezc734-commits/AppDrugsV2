using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
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
                return Result<bool>.Failure("Usuario no autenticado.");

            // Buscar el turno
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId && a.IsActive, cancellationToken);

            if (appointment == null)
                return Result<bool>.Failure($"El turno con ID {request.AppointmentId} no existe.");

            try
            {
                // Cambiar el estado según la transición
                switch (request.NewStatus)
                {
                    case AppointmentStatus.EnProceso:
                        appointment.Confirmar();
                        break;
                    case AppointmentStatus.Entregado:
                        appointment.Entregar();
                        break;
                    case AppointmentStatus.Cancelado:
                        appointment.Cancelar(request.Observaciones ?? "Cancelado por el gestor.");
                        break;
                    default:
                        return Result<bool>.Failure("Estado inválido para esta operación.");
                }

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