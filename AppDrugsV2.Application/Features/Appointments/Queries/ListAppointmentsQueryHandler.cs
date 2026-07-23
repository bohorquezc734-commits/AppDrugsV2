using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Appointments.DTOs;
using AppDrugsV2.Domain.Enums;

namespace AppDrugsV2.Application.Features.Appointments.Queries
{
    public class ListAppointmentsQueryHandler : IRequestHandler<ListAppointmentsQuery, List<AppointmentDto>>
    {
        private readonly IApplicationDbContext _context;

        public ListAppointmentsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AppointmentDto>> Handle(ListAppointmentsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.GestorFarmaceutico)
                .Include(a => a.Details)
                    .ThenInclude(d => d.Inventory)
                        .ThenInclude(i => i!.Drug)
                .Where(a => a.IsActive)
                .AsQueryable();

            if (request.UserId.HasValue)
                query = query.Where(a => a.UserId == request.UserId);

            if (request.GestorFarmaceuticoId.HasValue)
                query = query.Where(a => a.GestorFarmaceuticoId == request.GestorFarmaceuticoId);

            if (request.Status.HasValue)
                query = query.Where(a => a.Status == request.Status);

            if (request.FromDate.HasValue)
                query = query.Where(a => a.CreatedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(a => a.CreatedAt <= request.ToDate.Value);

            var appointments = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User != null ? $"{a.User.FullName}" : string.Empty,
                    GestorFarmaceuticoId = a.GestorFarmaceuticoId,
                    SedeName = a.GestorFarmaceutico != null ? a.GestorFarmaceutico.NombreSede! : string.Empty,
                    Status = a.Status,
                    ArchivoNombre = a.ArchivoNombre,
                    CreatedAt = a.CreatedAt,
                    FechaEntrega = a.FechaEntrega,
                    Observaciones = a.Observaciones,
                    IsActive = a.IsActive,
                    Details = a.Details.Select(d => new AppointmentDetailDto
                    {
                        Id = d.Id,
                        AppointmentId = d.AppointmentId,
                        InventoryId = d.InventoryId,
                        DrugName = d.Inventory != null && d.Inventory.Drug != null ? d.Inventory.Drug.Name : string.Empty,
                        Quantity = d.Quantity,
                        CreatedAt = d.CreatedAt
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            return appointments;
        }
    }
}