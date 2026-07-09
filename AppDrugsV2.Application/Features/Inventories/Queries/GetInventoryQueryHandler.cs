using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Inventories.DTOs;

namespace AppDrugsV2.Application.Features.Inventories.Queries
{
    public class GetInventoryQueryHandler : IRequestHandler<GetInventoryQuery, InventoryDto>
    {
        private readonly IApplicationDbContext _context;

        public GetInventoryQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryDto> Handle(GetInventoryQuery request, CancellationToken cancellationToken)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Drug)
                .Include(i => i.GestorFarmaceutico)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (inventory == null)
                throw new KeyNotFoundException($"Inventario con ID {request.Id} no encontrado");

            return new InventoryDto
            {
                Id = inventory.Id,
                DrugId = inventory.DrugId,
                DrugName = inventory.Drug?.Name ?? string.Empty,
                GestorFarmaceuticoId = inventory.GestorFarmaceuticoId,
                SedeName = inventory.GestorFarmaceutico?.NombreSede ?? string.Empty,
                Quantity = inventory.Quantity,
                IsActive = inventory.IsActive,
                CreatedAt = inventory.CreatedAt,
                UpdatedAt = inventory.UpdatedAt
            };
        }
    }
}