using MediatR;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Appointments.Commands
{
    /// <summary>
    /// Comando para generar el código QR de un turno específico.
    /// Retorna el QR en Base64 para que el cliente pueda mostrarlo
    /// sin necesidad de una segunda llamada.
    /// </summary>
    public class GenerateAppointmentQrCommand : IRequest<Result<string>>
    {
        /// <summary>ID del turno al que se le generará el QR.</summary>
        public int AppointmentId { get; set; }
    }
}
