using MediatR;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Inventories.Commands
{
    public class CreateInventoryCommand : IRequest<Result<int>>
    {
        public int DrugId { get; set; }
        public int GestorFarmaceuticoId { get; set; }
        public int Quantity { get; set; }
    }
}