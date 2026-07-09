using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Inventories.Commands
{
    public class UpdateInventoryCommandHandler : IRequestHandler<UpdateInventoryCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateInventoryCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == request.Id && i.IsActive, cancellationToken);

            if (inventory == null)
                return Result<bool>.Failure($"Inventario con ID {request.Id} no encontrado");

            try
            {
                inventory.UpdateStock(request.Quantity);
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