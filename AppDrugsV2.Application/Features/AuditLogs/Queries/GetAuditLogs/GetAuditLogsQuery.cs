using AppDrugsV2.Application.Common.Results;
using MediatR;
using System.Collections.Generic;

namespace AppDrugsV2.Application.Features.AuditLogs.Queries.GetAuditLogs
{
    public record GetAuditLogsQuery : IRequest<PagedResult<AuditLogDto>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
