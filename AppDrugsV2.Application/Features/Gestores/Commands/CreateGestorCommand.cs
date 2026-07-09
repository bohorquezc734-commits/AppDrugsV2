using MediatR;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Gestores.Commands
{
    public class CreateGestorCommand : IRequest<Result<int>>
    {
        public string NombreSede { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public int IdEps { get; set; }
    }
}