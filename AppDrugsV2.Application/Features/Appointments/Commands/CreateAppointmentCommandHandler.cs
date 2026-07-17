using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
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
            // 1. Validar autenticación
            if (!_currentUserService.IsAuthenticated)
                return Result<int>.Failure("Usuario no autenticado.");

            var userId = _currentUserService.UserId.Value;

            // 2. Validar que la sede existe
            var gestor = await _context.GestoresFarmaceuticos
                .FirstOrDefaultAsync(g => g.Id == request.GestorFarmaceuticoId && g.IsActive, cancellationToken);
            if (gestor == null)
                return Result<int>.Failure($"La sede con ID {request.GestorFarmaceuticoId} no existe o está inactiva.");

            // 3. Crear el turno
            var appointment = new Appointment(
                userId,
                request.GestorFarmaceuticoId,
                request.ArchivoAutorizacion,
                request.ArchivoNombre,
                request.ArchivoContentType
            );

            // 4. Guardar el turno primero para obtener el Id generado por la BD
            await _context.Appointments.AddAsync(appointment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // 5. Agregar detalles (medicamentos) usando el Id real del turno
            if (request.Details != null && request.Details.Any())
            {
                foreach (var detail in request.Details)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.Id == detail.InventoryId && i.IsActive, cancellationToken);

                    if (inventory == null)
                        return Result<int>.Failure($"El inventario con ID {detail.InventoryId} no existe.");

                    if (inventory.Quantity < detail.Quantity)
                        return Result<int>.Failure($"No hay suficiente stock. Stock disponible: {inventory.Quantity}.");

                    var appointmentDetail = new AppointmentDetail(
                        appointment.Id,   // ← Ahora el Id es el real de la BD
                        detail.InventoryId,
                        detail.Quantity
                    );

                    inventory.RemoveStock(detail.Quantity);
                    appointment.AddDetail(appointmentDetail);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }

            return Result<int>.Success(appointment.Id);
        }
    }
}