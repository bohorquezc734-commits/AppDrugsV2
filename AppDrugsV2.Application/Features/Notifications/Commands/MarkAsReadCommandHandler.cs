using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Common.Constants;

namespace AppDrugsV2.Application.Features.Notifications.Commands
{
    public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public MarkAsReadCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<bool>> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId == null)
            {
                return Result<bool>.Failure(AppConstants.Messages.UserNotAuthenticatedEn);
            }

            var userId = _currentUserService.UserId.Value;

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == request.Id && n.UserId == userId, cancellationToken);

            if (notification == null)
            {
                return Result<bool>.Failure(AppConstants.Messages.NotificationNotFound);
            }

            notification.MarkAsRead();
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
