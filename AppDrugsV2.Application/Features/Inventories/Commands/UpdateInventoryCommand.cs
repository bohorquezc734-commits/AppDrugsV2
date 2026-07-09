using MediatR;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Inventories.Commands
{
    public class UpdateInventoryCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }
}