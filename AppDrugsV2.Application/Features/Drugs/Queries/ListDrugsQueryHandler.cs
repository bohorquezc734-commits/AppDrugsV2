using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Drugs.DTOs;

namespace AppDrugsV2.Application.Features.Drugs.Queries
{
    public class ListDrugsQueryHandler : IRequestHandler<ListDrugsQuery, List<DrugDto>>
    {
        private readonly IApplicationDbContext _context;

        public ListDrugsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DrugDto>> Handle(ListDrugsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Drugs
                .Where(d => d.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(d =>
                    d.Name.Contains(request.SearchTerm) ||
                    d.GenericName.Contains(request.SearchTerm) ||
                    d.Laboratory.Contains(request.SearchTerm));
            }

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                query = query.Where(d => d.Category == request.Category);
            }

            if (request.RequiresPrescription.HasValue)
            {
                query = query.Where(d => d.RequiresPrescription == request.RequiresPrescription);
            }

            var drugs = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return drugs.Select(d => new DrugDto
            {
                Id = d.Id,
                Name = d.Name,
                GenericName = d.GenericName,
                Laboratory = d.Laboratory,
                Price = d.Price,
                Stock = d.Stock,
                Category = d.Category,
                RequiresPrescription = d.RequiresPrescription,
                ExpirationDate = d.ExpirationDate,
                IsExpired = d.IsExpired(),
                IsActive = d.IsActive
            }).ToList();
        }
    }
}