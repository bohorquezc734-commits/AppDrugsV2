using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Features.Auth.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppDrugsV2.Application.Features.Auth.Queries
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<PagedResult<UserDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetAllUsersQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            // Filtro por nombre o correo
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term));
            }

            // Filtro por rol
            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                query = query.Where(u => u.Role.ToString() == request.Role);
            }

            // Filtro por estado activo
            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.Page < 1 ? 1 : request.Page;
            var pageSize   = request.PageSize < 1 ? 20 : (request.PageSize > 100 ? 100 : request.PageSize);

            var users = await query
                .OrderBy(u => u.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto
                {
                    Id          = u.Id,
                    Email       = u.Email,
                    FullName    = u.FullName,
                    Role        = u.Role.ToString(),
                    IsActive    = u.IsActive,
                    CreatedAt   = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .ToListAsync(cancellationToken);

            return Result<PagedResult<UserDto>>.Success(
                new PagedResult<UserDto>(users, totalCount, pageNumber, pageSize));
        }
    }
}
