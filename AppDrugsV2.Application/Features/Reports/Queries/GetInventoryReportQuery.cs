using MediatR;
using AppDrugsV2.Application.Features.Reports.DTOs;

namespace AppDrugsV2.Application.Features.Reports.Queries
{
    public class GetInventoryReportQuery : IRequest<List<InventoryReportDto>>
    {
        public int? GestorFarmaceuticoId { get; set; }
        /// <summary>
        /// Si es true, solo devuelve registros activos. Si es null, devuelve todos.
        /// </summary>
        public bool? OnlyActive { get; set; }
        /// <summary>
        /// Si es true, solo devuelve registros con stock bajo (menor a 10).
        /// </summary>
        public bool? OnlyLowStock { get; set; }
    }
}
