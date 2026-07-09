using MediatR;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Drugs.Commands
{
    public class DeleteDrugCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }
}