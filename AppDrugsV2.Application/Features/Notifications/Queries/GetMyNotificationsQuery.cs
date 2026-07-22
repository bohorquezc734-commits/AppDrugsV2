using MediatR;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Notifications.Queries
{
    public class GetMyNotificationsQuery : IRequest<Result<List<NotificationDto>>>
    {
    }
}
