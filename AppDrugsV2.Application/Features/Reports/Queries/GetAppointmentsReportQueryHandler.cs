using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Reports.DTOs;
using AppDrugsV2.Domain.Enums;

namespace AppDrugsV2.Application.Features.Reports.Queries
{
    public class GetAppointmentsReportQueryHandler
        : IRequestHandler<GetAppointmentsReportQuery, List<AppointmentReportDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAppointmentsReportQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AppointmentReportDto>> Handle(
            GetAppointmentsReportQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.GestorFarmaceutico)
                .Include(a => a.Details)
                    .ThenInclude(d => d.Inventory)
                        .ThenInclude(i => i!.Drug)
                .AsNoTracking()
                .AsQueryable();

            // Filtros opcionales
            if (request.DateFrom.HasValue)
                query = query.Where(a => a.CreatedAt >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
                query = query.Where(a => a.CreatedAt <= request.DateTo.Value.AddDays(1));

            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<AppointmentStatus>(request.Status, ignoreCase: true, out var status))
            {
                var filterStatus = status;
                query = query.Where(a => a.Status == filterStatus);
            }

            if (request.GestorFarmaceuticoId.HasValue)
                query = query.Where(a => a.GestorFarmaceuticoId == request.GestorFarmaceuticoId.Value);

            var appointments = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);

            return appointments.Select(a => new AppointmentReportDto
            {
                Id = a.Id,
                NombreUsuario = a.User != null
                    ? a.User.FullName
                    : $"Usuario #{a.UserId}",
                Sede = a.GestorFarmaceutico?.NombreSede ?? $"Sede #{a.GestorFarmaceuticoId}",
                Estado = a.Status.ToString(),
                FechaCreacion = a.CreatedAt,
                FechaEntrega = a.FechaEntrega,
                ArchivoAdjunto = a.ArchivoNombre,
                Observaciones = a.Observaciones,
                Medicamentos = a.Details
                    .Select(d => d.Inventory?.Drug != null
                        ? $"{d.Inventory.Drug.Name} (x{d.Quantity})"
                        : $"Inventario #{d.InventoryId} (x{d.Quantity})")
                    .ToList()
            }).ToList();
        }
    }
}
