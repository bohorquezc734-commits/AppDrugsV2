using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AppDrugsV2.Application.Features.Notifications.Queries;
using AppDrugsV2.Application.Features.Notifications.Commands;

namespace AppDrugsV2.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var result = await _mediator.Send(new GetMyNotificationsQuery());
            
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(new { error = result.Error });
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _mediator.Send(new MarkAsReadCommand { Id = id });

            if (result.IsSuccess)
                return Ok();

            return BadRequest(new { error = result.Error });
        }
    }
}
