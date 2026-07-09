using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Drugs.DTOs;

namespace AppDrugsV2.Application.Features.Drugs.Queries
{
    public class GetDrugQueryHandler : IRequestHandler<GetDrugQuery, DrugDto>
    {
        private readonly IApplicationDbContext _context;

        public GetDrugQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DrugDto> Handle(GetDrugQuery request, CancellationToken cancellationToken)
        {
            var drug = await _context.Drugs
                .FirstOrDefaultAsync(d => d.Id == request.Id && d.IsActive, cancellationToken);

            if (drug == null)
                throw new KeyNotFoundException($"Medicamento con ID {request.Id} no encontrado");

            return new DrugDto
            {
                Id = drug.Id,
                Name = drug.Name,
                GenericName = drug.GenericName,
                Laboratory = drug.Laboratory,
                Price = drug.Price,
                Stock = drug.Stock,
                Category = drug.Category,
                RequiresPrescription = drug.RequiresPrescription,
                ExpirationDate = drug.ExpirationDate,
                IsExpired = drug.IsExpired(),
                IsActive = drug.IsActive
            };
        }
    }
}