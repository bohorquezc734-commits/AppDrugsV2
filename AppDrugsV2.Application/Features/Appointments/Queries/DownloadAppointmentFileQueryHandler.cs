using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Appointments.Queries
{
    public class DownloadAppointmentFileQueryHandler : IRequestHandler<DownloadAppointmentFileQuery, Result<FileResultDto>>
    {
        private readonly IApplicationDbContext _context;

        public DownloadAppointmentFileQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<FileResultDto>> Handle(DownloadAppointmentFileQuery request, CancellationToken cancellationToken)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId && a.IsActive, cancellationToken);

            if (appointment == null)
                return Result<FileResultDto>.Failure($"El turno con ID {request.AppointmentId} no existe.");

            if (appointment.ArchivoAutorizacion == null || appointment.ArchivoAutorizacion.Length == 0)
                return Result<FileResultDto>.Failure("Este turno no tiene un archivo adjunto.");

            var fileDto = new FileResultDto
            {
                Content = appointment.ArchivoAutorizacion,
                ContentType = appointment.ArchivoContentType ?? "application/octet-stream",
                FileName = appointment.ArchivoNombre ?? $"turno_{request.AppointmentId}_archivo"
            };

            return Result<FileResultDto>.Success(fileDto);
        }
    }
}
