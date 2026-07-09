using MediatR;
using AppDrugsV2.Application.Features.Inventories.DTOs;

namespace AppDrugsV2.Application.Features.Inventories.Queries
{
    public class GetInventoryQuery : IRequest<InventoryDto>
    {
        public int Id { get; set; }
    }
}