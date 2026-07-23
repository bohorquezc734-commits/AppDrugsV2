using MediatR;
using System.Collections.Generic;

namespace AppDrugsV2.Application.Features.AuditLogs.Queries.GetAuditLogs
{
    public record GetAuditLogsQuery : IRequest<List<AuditLogDto>>;
}
