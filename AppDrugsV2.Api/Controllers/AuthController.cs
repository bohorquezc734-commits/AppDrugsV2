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
                return Ok(result.Value);

            return Unauthorized(new { error = result.Error });
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
