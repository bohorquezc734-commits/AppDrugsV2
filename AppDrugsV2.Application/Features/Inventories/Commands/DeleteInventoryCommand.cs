using MediatR;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Inventories.Commands
{
    public class DeleteInventoryCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }
}