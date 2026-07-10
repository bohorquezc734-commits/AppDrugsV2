using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Features.Gestores.Commands;
using AppDrugsV2.Application.Features.Gestores.Queries;
using AppDrugsV2.Application.Common.Results;

namespace AppDrugsV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GestoresController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GestoresController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = AppConstants.Roles.AdminOrPharmacist)]
        public async Task<IActionResult> GetAll([FromQuery] ListGestoresQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = AppConstants.Roles.AdminOrPharmacist)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _mediator.Send(new GetGestorQuery { Id = id });
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<IActionResult> Create([FromBody] CreateGestorCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { gestorId = result.Value });

            return BadRequest(new { error = result.Error });
        }
    }
}