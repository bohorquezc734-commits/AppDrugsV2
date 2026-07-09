using MediatR;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Drugs.Commands
{
    public class UpdateDrugCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public string Laboratory { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool RequiresPrescription { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}