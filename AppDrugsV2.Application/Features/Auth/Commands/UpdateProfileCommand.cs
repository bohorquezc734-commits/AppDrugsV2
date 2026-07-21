using MediatR;
using AppDrugsV2.Application.Common.Results;
using System.Text.Json.Serialization;

namespace AppDrugsV2.Application.Features.Auth.Commands
{
    public class UpdateProfileCommand : IRequest<Result<bool>>
    {
        [JsonIgnore]
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
