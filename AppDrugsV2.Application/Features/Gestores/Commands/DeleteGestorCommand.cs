using MediatR;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Common.Constants;

namespace AppDrugsV2.Application.Features.Gestores.Commands
{
    public class DeleteGestorCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteGestorCommandHandler : IRequestHandler<DeleteGestorCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteGestorCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(DeleteGestorCommand request, CancellationToken cancellationToken)
        {
            var gestor = await _context.GestoresFarmaceuticos.FindAsync(new object[] { request.Id }, cancellationToken);

            if (gestor == null)
                return Result<bool>.Failure($"Sede {AppConstants.Messages.NotFoundKeyword}");

            gestor.Deactivate(); // Soft Delete existente en tu dominio
            
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
