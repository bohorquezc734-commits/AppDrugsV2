using MediatR;
using AppDrugsV2.Application.Features.Drugs.DTOs;

namespace AppDrugsV2.Application.Features.Drugs.Queries
{
    public class ListDrugsQuery : IRequest<List<DrugDto>>
    {
        public string? SearchTerm { get; set; }
        public string? Category { get; set; }
        public bool? RequiresPrescription { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}