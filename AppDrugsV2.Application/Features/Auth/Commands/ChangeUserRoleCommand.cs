using MediatR;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Domain.Enums;

namespace AppDrugsV2.Application.Features.Auth.Commands
{
    public class ChangeUserRoleCommand : IRequest<Result<bool>>
    {
        public int UserId { get; set; }
        public UserRole NewRole { get; set; }
    }

    public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public ChangeUserRoleCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

            if (user == null)
                return Result<bool>.Failure($"Usuario {AppConstants.Messages.NotFoundKeyword}");

            user.UpdateRole(request.NewRole);
            
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
