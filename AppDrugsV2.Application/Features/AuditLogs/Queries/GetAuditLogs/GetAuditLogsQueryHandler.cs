using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppDrugsV2.Application.Features.AuditLogs.Queries.GetAuditLogs
{
    public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAuditLogsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        {
            var query = from a in _context.AuditLogs.AsNoTracking()
                        join u in _context.Users.AsNoTracking() on a.UserId equals u.Id into userGroup
                        from user in userGroup.DefaultIfEmpty()
                        orderby a.Timestamp descending
                        select new AuditLogDto
                        {
                            Id = a.Id,
                            UserId = a.UserId,
                            UserName = a.UserId != null && user != null ? user.FullName : "Sistema",
                            Action = a.Action,
                            EntityName = a.EntityName,
                            PrimaryKey = a.PrimaryKey,
                            OldValues = a.OldValues,
                            NewValues = a.NewValues,
                            Timestamp = a.Timestamp
                        };

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : (request.PageSize > 100 ? 100 : request.PageSize);

            var items = await query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync(cancellationToken);

            return new PagedResult<AuditLogDto>(items, totalCount, pageNumber, pageSize);
        }
    }
}
