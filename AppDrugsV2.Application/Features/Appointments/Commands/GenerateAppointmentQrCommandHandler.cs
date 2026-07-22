using MediatR;
using Microsoft.EntityFrameworkCore;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Application.Features.Appointments.Commands
{
    /// <summary>
    /// Orquesta la generación del QR para un turno:
    ///   1. Valida que el turno exista.
    ///   2. Genera el QR usando <see cref="IQrCodeService"/> (sin acoplarse a la librería).
    ///   3. Persiste el QR en la entidad via el método de dominio AssignQrCode().
    ///   4. Notifica en tiempo real al usuario via <see cref="INotificationHubService"/>
    ///      (sin acoplarse a SignalR).
    /// </summary>
    public class GenerateAppointmentQrCommandHandler
        : IRequestHandler<GenerateAppointmentQrCommand, Result<string>>
    {
        private readonly IApplicationDbContext    _context;
        private readonly IQrCodeService           _qrCodeService;
        private readonly INotificationHubService  _notificationHub;

        public GenerateAppointmentQrCommandHandler(
            IApplicationDbContext   context,
            IQrCodeService          qrCodeService,
            INotificationHubService notificationHub)
        {
            _context         = context;
            _qrCodeService   = qrCodeService;
            _notificationHub = notificationHub;
        }

        public async Task<Result<string>> Handle(
            GenerateAppointmentQrCommand request,
            CancellationToken cancellationToken)
        {
            // ── 1. Buscar el turno ────────────────────────────────────────────────
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(
                    a => a.Id == request.AppointmentId && a.IsActive,
                    cancellationToken);

            if (appointment is null)
                return Result<string>.Failure(
                    $"El turno con ID {request.AppointmentId} {AppConstants.Messages.NotExistsKeyword}.");

            // ── 2. Generar el código QR ───────────────────────────────────────────
            // El contenido codificado puede ser una URL al detalle del turno,
            // un JSON resumen, o cualquier identificador que decida el negocio.
            var qrContent  = $"APPDRUGS|TURNO:{appointment.Id}|USUARIO:{appointment.UserId}|FECHA:{appointment.CreatedAt:yyyy-MM-dd}";
            var qrBase64   = _qrCodeService.GenerateBase64(qrContent);

            // ── 3. Persistir usando el método de dominio (encapsulamiento) ────────
            try
            {
                appointment.AssignQrCode(qrBase64);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (ArgumentException ex)
            {
                return Result<string>.Failure(ex.Message);
            }

            // ── 4. Notificar al usuario en tiempo real (fire-and-forget seguro) ──
            // No lanzamos excepción si SignalR falla; el QR ya fue persistido.
            try
            {
                await _notificationHub.SendToUserAsync(
                    userId:            appointment.UserId,
                    eventName:         "QrReady",
                    payload: new
                    {
                        appointmentId = appointment.Id,
                        message       = $"✅ El QR de tu turno #{appointment.Id} está listo.",
                        qrBase64
                    },
                    cancellationToken: cancellationToken);
            }
            catch
            {
                // SignalR no disponible → no bloqueamos el flujo principal.
            }

            return Result<string>.Success(qrBase64);
        }
    }
}
