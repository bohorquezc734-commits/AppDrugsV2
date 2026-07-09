using MediatR;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Inventories.Commands
{
    public class RemoveStockCommand : IRequest<Result<bool>>
    {
        public int InventoryId { get; set; }
        public int Quantity { get; set; }
    }
}