using AppDrugsV2.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppDrugsV2.Application.Features.AuditLogs.Queries.GetAuditLogs
{
    public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, List<AuditLogDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAuditLogsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        {
            return await (from a in _context.AuditLogs.AsNoTracking()
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
                          })
                          .ToListAsync(cancellationToken);
        }
    }
}
