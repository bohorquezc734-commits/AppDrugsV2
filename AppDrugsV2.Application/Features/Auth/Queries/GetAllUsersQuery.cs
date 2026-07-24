using MediatR;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Features.Auth.DTOs;
using System.Collections.Generic;

namespace AppDrugsV2.Application.Features.Auth.Queries
{
    public class GetAllUsersQuery : IRequest<Result<List<UserDto>>>
    {
    }
}
