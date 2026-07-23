using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Features.Drugs.Commands;
using AppDrugsV2.Application.Features.Drugs.Queries;

namespace AppDrugsV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DrugsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DrugsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] ListDrugsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetDrugQuery { Id = id };
            try
            {
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.Roles.AdminOrPharmacist)]
        public async Task<IActionResult> Create([FromBody] CreateDrugCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { drugId = result.Value });

            return BadRequest(new { error = result.Error });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AppConstants.Roles.AdminOrPharmacist)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDrugCommand command)
        {
            if (id != command.Id)
                return BadRequest(new { error = AppConstants.Messages.IdMismatch });

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(new { message = AppConstants.Messages.DrugUpdated });

            if (result.Error!.Contains(AppConstants.Messages.NotFoundKeyword))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteDrugCommand { Id = id };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(new { message = AppConstants.Messages.DrugDeleted });

            if (result.Error!.Contains(AppConstants.Messages.NotFoundKeyword))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        [HttpPatch("{id}/stock")]
        [Authorize(Roles = AppConstants.Roles.AdminOrPharmacist)]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockCommand command)
        {
            if (id != command.DrugId)
                return BadRequest(new { error = AppConstants.Messages.IdMismatch });

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(new { message = AppConstants.Messages.StockUpdated });

            if (result.Error!.Contains(AppConstants.Messages.NotFoundKeyword))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }
    }
}