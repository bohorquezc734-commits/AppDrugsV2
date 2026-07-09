using MediatR;
using AppDrugsV2.Application.Features.Gestores.DTOs;

namespace AppDrugsV2.Application.Features.Gestores.Queries
{
    public class GetGestorQuery : IRequest<GestorDto>
    {
        public int Id { get; set; }
    }
}