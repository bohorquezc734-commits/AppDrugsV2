using MediatR;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Common.Constants;

namespace AppDrugsV2.Application.Features.Auth.Commands
{
    public class ToggleUserStatusCommand : IRequest<Result<bool>>
    {
        public int UserId { get; set; }
        public bool IsActive { get; set; }
    }

    public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public ToggleUserStatusCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

            if (user == null)
                return Result<bool>.Failure($"Usuario {AppConstants.Messages.NotFoundKeyword}");

            if (request.IsActive)
                user.Activate();
            else
                user.Deactivate();
            
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
