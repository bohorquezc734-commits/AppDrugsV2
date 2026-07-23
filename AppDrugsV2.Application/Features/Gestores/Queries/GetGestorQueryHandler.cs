using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Gestores.DTOs;

namespace AppDrugsV2.Application.Features.Gestores.Queries
{
    public class GetGestorQueryHandler : IRequestHandler<GetGestorQuery, GestorDto>
    {
        private readonly IApplicationDbContext _context;

        public GetGestorQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GestorDto> Handle(GetGestorQuery request, CancellationToken cancellationToken)
        {
            var gestor = await _context.GestoresFarmaceuticos
                .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

            if (gestor == null)
                throw new KeyNotFoundException($"Gestor farmacéutico con ID {request.Id} no encontrado");

            return new GestorDto
            {
                Id = gestor.Id,
                NombreSede = gestor.NombreSede!,
                Direccion = gestor.Direccion!,
                Telefono = gestor.Telefono!,
                IdEps = gestor.IdEps,
                IsActive = gestor.IsActive,
                CreatedAt = gestor.CreatedAt
            };
        }
    }
}