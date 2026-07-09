using MediatR;
using AppDrugsV2.Application.Features.Inventories.DTOs;

namespace AppDrugsV2.Application.Features.Inventories.Queries
{
    public class ListInventoriesQuery : IRequest<List<InventoryDto>>
    {
        public int? GestorFarmaceuticoId { get; set; }
        public int? DrugId { get; set; }
        public int? MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}