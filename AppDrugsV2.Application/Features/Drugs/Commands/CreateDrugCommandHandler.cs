using MediatR;
using AppDrugsV2.Domain.Entities;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Drugs.Commands
{
    public class CreateDrugCommandHandler : IRequestHandler<CreateDrugCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;

        public CreateDrugCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(CreateDrugCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var drug = new Drug(
                    request.Name,
                    request.GenericName,
                    request.Laboratory,
                    request.Price,
                    request.Stock,
                    request.Category,
                    request.RequiresPrescription,
                    request.ExpirationDate
                );

                await _context.Drugs.AddAsync(drug, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return Result<int>.Success(drug.Id);
            }
            catch (ArgumentException ex)
            {
                return Result<int>.Failure(ex.Message);
            }
        }
    }
}