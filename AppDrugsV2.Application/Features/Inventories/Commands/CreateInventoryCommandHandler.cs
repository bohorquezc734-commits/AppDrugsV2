using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Domain.Entities;

namespace AppDrugsV2.Application.Features.Inventories.Commands
{
    public class CreateInventoryCommandHandler : IRequestHandler<CreateInventoryCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;

        public CreateInventoryCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(CreateInventoryCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el medicamento existe
            var drugExists = await _context.Drugs.AnyAsync(d => d.Id == request.DrugId && d.IsActive, cancellationToken);
            if (!drugExists)
                return Result<int>.Failure($"Medicamento con ID {request.DrugId} no encontrado");

            // Verificar que la sede existe
            var gestorExists = await _context.GestoresFarmaceuticos.AnyAsync(g => g.Id == request.GestorFarmaceuticoId && g.IsActive, cancellationToken);
            if (!gestorExists)
                return Result<int>.Failure($"Sede con ID {request.GestorFarmaceuticoId} no encontrada");

            // Verificar que no exista un inventario para este medicamento en esta sede
            var existingInventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.DrugId == request.DrugId && i.GestorFarmaceuticoId == request.GestorFarmaceuticoId, cancellationToken);

            if (existingInventory != null)
                return Result<int>.Failure("Ya existe un inventario para este medicamento en esta sede");

            try
            {
                var inventory = new Inventory(
                    request.DrugId,
                    request.GestorFarmaceuticoId,
                    request.Quantity
                );

                await _context.Inventories.AddAsync(inventory, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return Result<int>.Success(inventory.Id);
            }
            catch (ArgumentException ex)
            {
                return Result<int>.Failure(ex.Message);
            }
        }
    }
}