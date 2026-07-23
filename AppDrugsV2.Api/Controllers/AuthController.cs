using MediatR;
using Microsoft.AspNetCore.Mvc;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Features.Auth.Commands;
using AppDrugsV2.Application.Features.Auth.Queries;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AppDrugsV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(new { userId = result.Value, message = AppConstants.Messages.UserRegistered });

            return BadRequest(new { error = result.Error });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginQuery query)
        {
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = result.Value.ExpiresAt
                };
                
                Response.Cookies.Append("X-Access-Token", result.Value.Token, cookieOptions);

                return Ok(new 
                {
                    result.Value.UserId,
                    result.Value.FullName,
                    result.Value.Role,
                    result.Value.ExpiresAt
                });
            }

            return Unauthorized(new { error = result.Error });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Append("X-Access-Token", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1)
            });

            return Ok(new { message = "Sesión cerrada correctamente." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            var result = await _mediator.Send(command);
            
            // Siempre retornamos Ok para no revelar qué correos están registrados
            return Ok(new { message = "Si el correo está registrado, se ha enviado un código de recuperación." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(new { message = "Contraseña restablecida exitosamente." });

            return BadRequest(new { error = result.Error });
        }
        [Authorize]
        [HttpPut("me/password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            command.UserId = userId;
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(new { message = AppConstants.Messages.PasswordChanged });

            return BadRequest(new { error = result.Error });
        }

        [Authorize]
        [HttpPut("me/profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            command.UserId = userId;
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(new { message = AppConstants.Messages.ProfileUpdated });

            return BadRequest(new { error = result.Error });
        }
    }
}
