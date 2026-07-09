using MediatR;
using Microsoft.AspNetCore.Mvc;
using AppDrugsV2.Application.Features.Drugs.Commands;
using AppDrugsV2.Application.Features.Drugs.Queries;

namespace AppDrugsV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrugsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DrugsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListDrugsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDrugCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetAll), new { id = result.Value }, new { drugId = result.Value });

            return BadRequest(new { error = result.Error });
        }
    }
}