using MediatR;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Appointments.Queries
{
    public class DownloadAppointmentFileQuery : IRequest<Result<FileResultDto>>
    {
        public int AppointmentId { get; set; }
    }

    public class FileResultDto
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
