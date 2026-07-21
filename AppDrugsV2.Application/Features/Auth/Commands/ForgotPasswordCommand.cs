using MediatR;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Auth.Commands
{
    public class ForgotPasswordCommand : IRequest<Result<bool>>
    {
        public string Email { get; set; } = string.Empty;
    }
}
