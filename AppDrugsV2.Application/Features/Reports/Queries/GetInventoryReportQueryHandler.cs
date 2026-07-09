using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Reports.DTOs;

namespace AppDrugsV2.Application.Features.Reports.Queries
{
    public class GetInventoryReportQueryHandler
        : IRequestHandler<GetInventoryReportQuery, List<InventoryReportDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetInventoryReportQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryReportDto>> Handle(
            GetInventoryReportQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Inventories
                .Include(i => i.Drug)
                .Include(i => i.GestorFarmaceutico)
                .AsNoTracking()
                .AsQueryable();

            if (request.GestorFarmaceuticoId.HasValue)
                query = query.Where(i => i.GestorFarmaceuticoId == request.GestorFarmaceuticoId.Value);

            if (request.OnlyActive.HasValue)
                query = query.Where(i => i.IsActive == request.OnlyActive.Value);

            if (request.OnlyLowStock == true)
                query = query.Where(i => i.Quantity < 10);

            var inventories = await query
                .OrderBy(i => i.GestorFarmaceuticoId)
                .ThenBy(i => i.Drug!.Name)
                .ToListAsync(cancellationToken);

            return inventories.Select(i => new InventoryReportDto
            {
                Id = i.Id,
                Medicamento = i.Drug?.Name ?? $"Medicamento #{i.DrugId}",
                Sede = i.GestorFarmaceutico?.NombreSede ?? $"Sede #{i.GestorFarmaceuticoId}",
                Stock = i.Quantity,
                Activo = i.IsActive,
                FechaCreacion = i.CreatedAt,
                FechaActualizacion = i.UpdatedAt
            }).ToList();
        }
    }
}
