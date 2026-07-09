using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Drugs.Commands
{
    public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateStockCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
        {
            var drug = await _context.Drugs
                .FirstOrDefaultAsync(d => d.Id == request.DrugId && d.IsActive, cancellationToken);

            if (drug == null)
                return Result<bool>.Failure($"Medicamento con ID {request.DrugId} no encontrado");

            try
            {
                drug.UpdateStock(request.Quantity);
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