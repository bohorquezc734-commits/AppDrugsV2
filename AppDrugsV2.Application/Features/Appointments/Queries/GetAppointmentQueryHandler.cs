using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Appointments.DTOs;

namespace AppDrugsV2.Application.Features.Appointments.Queries
{
    public class GetAppointmentQueryHandler : IRequestHandler<GetAppointmentQuery, AppointmentDto>
    {
        private readonly IApplicationDbContext _context;

        public GetAppointmentQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AppointmentDto> Handle(GetAppointmentQuery request, CancellationToken cancellationToken)
        {
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.GestorFarmaceutico)
                .Include(a => a.Details)
                    .ThenInclude(d => d.Inventory)
                        .ThenInclude(i => i!.Drug)
                .FirstOrDefaultAsync(a => a.Id == request.Id && a.IsActive, cancellationToken);

            if (appointment == null)
                throw new KeyNotFoundException($"El turno con ID {request.Id} no existe.");

            return new AppointmentDto
            {
                Id = appointment.Id,
                UserId = appointment.UserId,
                UserName = appointment.User?.FullName ?? string.Empty,
                GestorFarmaceuticoId = appointment.GestorFarmaceuticoId,
                SedeName = appointment.GestorFarmaceutico?.NombreSede ?? string.Empty,
                Status = appointment.Status,
                ArchivoNombre = appointment.ArchivoNombre,
                CreatedAt = appointment.CreatedAt,
                FechaEntrega = appointment.FechaEntrega,
                Observaciones = appointment.Observaciones,
                IsActive = appointment.IsActive,
                Details = appointment.Details.Select(d => new AppointmentDetailDto
                {
                    Id = d.Id,
                    AppointmentId = d.AppointmentId,
                    InventoryId = d.InventoryId,
                    DrugName = d.Inventory?.Drug?.Name ?? string.Empty,
                    Quantity = d.Quantity,
                    CreatedAt = d.CreatedAt
                }).ToList()
            };
        }
    }
}