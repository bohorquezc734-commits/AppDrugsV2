using MediatR;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Notifications.Commands
{
    public class MarkAsReadCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }
}
