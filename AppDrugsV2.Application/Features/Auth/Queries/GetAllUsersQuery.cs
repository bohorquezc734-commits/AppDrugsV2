using MediatR;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Features.Auth.DTOs;

namespace AppDrugsV2.Application.Features.Auth.Queries
{
    public class GetAllUsersQuery : IRequest<Result<PagedResult<UserDto>>>
    {
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
