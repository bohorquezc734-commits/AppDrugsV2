using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Inventories.Commands
{
    public class AddStockCommandHandler : IRequestHandler<AddStockCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public AddStockCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(AddStockCommand request, CancellationToken cancellationToken)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == request.InventoryId && i.IsActive, cancellationToken);

            if (inventory == null)
                return Result<bool>.Failure($"Inventario con ID {request.InventoryId} no encontrado");

            try
            {
                inventory.AddStock(request.Quantity);
                await _context.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
            catch (ArgumentException ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
        }
    }
}