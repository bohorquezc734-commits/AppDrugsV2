using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Gestores.DTOs;

namespace AppDrugsV2.Application.Features.Gestores.Queries
{
    public class ListGestoresQueryHandler : IRequestHandler<ListGestoresQuery, List<GestorDto>>
    {
        private readonly IApplicationDbContext _context;

        public ListGestoresQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<GestorDto>> Handle(ListGestoresQuery request, CancellationToken cancellationToken)
        {
            var query = _context.GestoresFarmaceuticos
                .Where(g => g.IsActive == (request.IsActive ?? true))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(g =>
                    g.NombreSede!.Contains(request.SearchTerm) ||
                    g.Direccion!.Contains(request.SearchTerm));
            }

            if (request.IdEps.HasValue)
            {
                query = query.Where(g => g.IdEps == request.IdEps.Value);
            }

            var gestores = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(g => new GestorDto
                {
                    Id = g.Id,
                    NombreSede = g.NombreSede!,
                    Direccion = g.Direccion!,
                    Telefono = g.Telefono!,
                    IdEps = g.IdEps,
                    IsActive = g.IsActive,
                    CreatedAt = g.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return gestores;
        }
    }
}