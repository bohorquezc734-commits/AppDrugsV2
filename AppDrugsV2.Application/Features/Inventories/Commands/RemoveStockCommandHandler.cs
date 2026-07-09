using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Inventories.Commands
{
    public class RemoveStockCommandHandler : IRequestHandler<RemoveStockCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public RemoveStockCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(RemoveStockCommand request, CancellationToken cancellationToken)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == request.InventoryId && i.IsActive, cancellationToken);

            if (inventory == null)
                return Result<bool>.Failure($"Inventario con ID {request.InventoryId} no encontrado");

            try
            {
                inventory.RemoveStock(request.Quantity);
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