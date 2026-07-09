using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Drugs.Commands
{
    public class DeleteDrugCommandHandler : IRequestHandler<DeleteDrugCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteDrugCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(DeleteDrugCommand request, CancellationToken cancellationToken)
        {
            var drug = await _context.Drugs
                .FirstOrDefaultAsync(d => d.Id == request.Id && d.IsActive, cancellationToken);

            if (drug == null)
                return Result<bool>.Failure($"Medicamento con ID {request.Id} no encontrado");

            drug.Deactivate();
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}