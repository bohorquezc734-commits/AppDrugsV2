using MediatR;
using AppDrugsV2.Application.Features.Drugs.DTOs;

namespace AppDrugsV2.Application.Features.Drugs.Queries
{
    public class GetDrugQuery : IRequest<DrugDto>
    {
        public int Id { get; set; }
    }
}