using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Inventories.DTOs;

namespace AppDrugsV2.Application.Features.Inventories.Queries
{
    public class ListInventoriesQueryHandler : IRequestHandler<ListInventoriesQuery, List<InventoryDto>>
    {
        private readonly IApplicationDbContext _context;

        public ListInventoriesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryDto>> Handle(ListInventoriesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Inventories
                .Include(i => i.Drug)
                .Include(i => i.GestorFarmaceutico)
                .Where(i => i.IsActive == (request.IsActive ?? true))
                .AsQueryable();

            if (request.GestorFarmaceuticoId.HasValue)
            {
                query = query.Where(i => i.GestorFarmaceuticoId == request.GestorFarmaceuticoId.Value);
            }

            if (request.DrugId.HasValue)
            {
                query = query.Where(i => i.DrugId == request.DrugId.Value);
            }

            if (request.MinQuantity.HasValue)
            {
                query = query.Where(i => i.Quantity >= request.MinQuantity.Value);
            }

            if (request.MaxQuantity.HasValue)
            {
                query = query.Where(i => i.Quantity <= request.MaxQuantity.Value);
            }

            var inventories = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(i => new InventoryDto
                {
                    Id = i.Id,
                    DrugId = i.DrugId,
                    DrugName = i.Drug != null ? i.Drug.Name : string.Empty,
                    GestorFarmaceuticoId = i.GestorFarmaceuticoId,
                    SedeName = i.GestorFarmaceutico != null ? i.GestorFarmaceutico.NombreSede! : string.Empty,
                    Quantity = i.Quantity,
                    IsActive = i.IsActive,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return inventories;
        }
    }
}