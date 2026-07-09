using MediatR;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Features.Auth.DTOs;

namespace AppDrugsV2.Application.Features.Auth.Queries
{
    public class LoginQuery : IRequest<Result<LoginResponse>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}