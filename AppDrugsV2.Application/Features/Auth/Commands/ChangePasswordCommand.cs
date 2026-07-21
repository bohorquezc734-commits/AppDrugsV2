using MediatR;
using AppDrugsV2.Application.Common.Results;
using System.Text.Json.Serialization;

namespace AppDrugsV2.Application.Features.Auth.Commands
{
    public class ChangePasswordCommand : IRequest<Result<bool>>
    {
        [JsonIgnore]
        public int UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
