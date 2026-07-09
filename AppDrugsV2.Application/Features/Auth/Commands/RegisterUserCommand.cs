using MediatR;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Auth.Commands
{
    public class RegisterUserCommand : IRequest<Result<int>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; }    = string.Empty;
        public string Role { get; set; } = "User";
    }
}

