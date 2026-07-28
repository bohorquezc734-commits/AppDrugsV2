using MediatR;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Common.Constants;

namespace AppDrugsV2.Application.Features.Gestores.Commands
{
    public class UpdateGestorCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }
        public string NombreSede { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public int IdEps { get; set; }
    }

    public class UpdateGestorCommandHandler : IRequestHandler<UpdateGestorCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateGestorCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(UpdateGestorCommand request, CancellationToken cancellationToken)
        {
            var gestor = await _context.GestoresFarmaceuticos.FindAsync(new object[] { request.Id }, cancellationToken);

            if (gestor == null)
                return Result<int>.Failure($"Sede {AppConstants.Messages.NotFoundKeyword}");

            gestor.Update(request.NombreSede, request.Direccion, request.Telefono, request.IdEps);
            
            await _context.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(gestor.Id);
        }
    }
}
