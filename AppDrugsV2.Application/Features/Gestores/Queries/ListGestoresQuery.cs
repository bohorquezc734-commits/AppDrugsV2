using MediatR;
using AppDrugsV2.Application.Features.Gestores.DTOs;

namespace AppDrugsV2.Application.Features.Gestores.Queries
{
    public class ListGestoresQuery : IRequest<List<GestorDto>>
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int? IdEps { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}