using MediatR;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Domain.Entities;

namespace AppDrugsV2.Application.Features.Gestores.Commands
{
    public class CreateGestorCommandHandler : IRequestHandler<CreateGestorCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;

        public CreateGestorCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(CreateGestorCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var gestor = new GestorFarmaceutico(
                    request.NombreSede,
                    request.Direccion,
                    request.Telefono,
                    request.IdEps
                );

                await _context.GestoresFarmaceuticos.AddAsync(gestor, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return Result<int>.Success(gestor.Id);
            }
            catch (ArgumentException ex)
            {
                return Result<int>.Failure(ex.Message);
            }
        }
    }
}