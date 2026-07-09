using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Inventories.Commands
{
    public class DeleteInventoryCommandHandler : IRequestHandler<DeleteInventoryCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteInventoryCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(DeleteInventoryCommand request, CancellationToken cancellationToken)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == request.Id && i.IsActive, cancellationToken);

            if (inventory == null)
                return Result<bool>.Failure($"Inventario con ID {request.Id} no encontrado");

            inventory.Deactivate();
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}