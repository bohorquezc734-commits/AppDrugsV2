using MediatR;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Drugs.Commands
{
    public class UpdateStockCommand : IRequest<Result<bool>>
    {
        public int DrugId { get; set; }
        public int Quantity { get; set; } // Positivo = agregar, Negativo = quitar
    }
}