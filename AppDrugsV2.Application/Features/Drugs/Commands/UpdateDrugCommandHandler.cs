using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Drugs.Commands
{
    public class UpdateDrugCommandHandler : IRequestHandler<UpdateDrugCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateDrugCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(UpdateDrugCommand request, CancellationToken cancellationToken)
        {
            var drug = await _context.Drugs
                .FirstOrDefaultAsync(d => d.Id == request.Id && d.IsActive, cancellationToken);

            if (drug == null)
                return Result<bool>.Failure($"Medicamento con ID {request.Id} no encontrado");

            try
            {
                drug.UpdateInfo(
                    request.Name,
                    request.GenericName,
                    request.Laboratory,
                    request.Category,
                    request.RequiresPrescription,
                    request.ExpirationDate
                );

                drug.UpdatePrice(request.Price);

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