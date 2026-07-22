using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Features.Appointments.Commands;
using AppDrugsV2.Application.Features.Appointments.Queries;

namespace AppDrugsV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AppointmentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListAppointmentsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("mis-turnos")]
        public async Task<IActionResult> GetMyAppointments([FromQuery] ListAppointmentsQuery query)
        {
            // El servicio CurrentUserService debe proveer el UserId
            // Asumimos que se inyecta o se obtiene de los claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userId, out int id))
                return Unauthorized();

            query.UserId = id;
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetAppointmentQuery { Id = id });
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.Roles.UserOrAdmin)]
        public async Task<IActionResult> Create([FromForm] CreateAppointmentCommand command)
        {
            // Procesar el archivo en el controlador (NO en Application)
            if (Request.Form.Files.Count > 0)
            {
                var file = Request.Form.Files[0];
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                command.ArchivoAutorizacion = memoryStream.ToArray();
                command.ArchivoNombre = file.FileName;
                command.ArchivoContentType = file.ContentType;
            }

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { appointmentId = result.Value });

            return BadRequest(new { error = result.Error });
        }
        [HttpPatch("{id}/status")]
        [Authorize(Roles = AppConstants.Roles.AdminOrPharmacist)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusCommand command)
        {
            if (id != command.AppointmentId)
                return BadRequest(new { error = AppConstants.Messages.IdMismatch });

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(new { message = AppConstants.Messages.AppointmentUpdated });

            if (result.Error.Contains(AppConstants.Messages.NotExistsKeyword))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        // ─── GENERACIÓN DE QR ─────────────────────────────────────────────────────

        /// <summary>
        /// Genera (o regenera) el código QR del turno especificado.
        /// Persiste el QR en la base de datos y notifica al usuario
        /// en tiempo real via SignalR (evento "QrReady").
        /// </summary>
        /// <param name="id">ID del turno.</param>
        /// <returns>200 con el QR en Base64 | 404 si el turno no existe | 400 si hay un error de validación.</returns>
        [HttpPost("{id}/qr")]
        [Authorize(Roles = AppConstants.Roles.AdminOrPharmacist)]
        public async Task<IActionResult> GenerateQr(int id)
        {
            var command = new GenerateAppointmentQrCommand { AppointmentId = id };
            var result  = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(new { appointmentId = id, qrBase64 = result.Value });

            if (result.Error != null && result.Error.Contains(AppConstants.Messages.NotExistsKeyword))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }
    }
}