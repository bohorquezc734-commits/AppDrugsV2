using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Common.Constants;

namespace AppDrugsV2.Application.Features.Auth.Commands
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public ResetPasswordCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

            if (user == null)
            {
                return Result<bool>.Failure(AppConstants.Messages.UserNotFound);
            }

            if (user.ResetPasswordToken != request.Token || 
                !user.ResetPasswordTokenExpiresAt.HasValue || 
                user.ResetPasswordTokenExpiresAt.Value < DateTime.UtcNow)
            {
                return Result<bool>.Failure("El código de recuperación es inválido o ha expirado.");
            }

            user.UpdatePassword(_passwordHasher.Hash(request.NewPassword));
            user.ClearResetPasswordToken();

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
