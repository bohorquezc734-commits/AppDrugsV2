using MediatR;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Appointments.Commands
{
    public class CreateAppointmentCommand : IRequest<Result<int>>
    {
        public int GestorFarmaceuticoId { get; set; }
        public byte[]? ArchivoAutorizacion { get; set; }
        public string? ArchivoNombre { get; set; }
        public string? ArchivoContentType { get; set; }
        public List<CreateAppointmentDetailCommand>? Details { get; set; }
    }

    public class CreateAppointmentDetailCommand
    {
        public int InventoryId { get; set; }
        public int Quantity { get; set; }
    }
}