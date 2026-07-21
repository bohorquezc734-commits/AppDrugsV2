using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Features.Inventories.Commands;
using AppDrugsV2.Application.Features.Inventories.Queries;

namespace AppDrugsV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InventoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize] // Todos los usuarios autenticados pueden ver inventario de sedes para crear turnos
        public async Task<IActionResult> GetAll([FromQuery] ListInventoriesQuery query)
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
                var result = await _mediator.Send(new GetInventoryQuery { Id = id });
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.Roles.AdminOrPharmacist)]
        public async Task<IActionResult> Create([FromBody] CreateInventoryCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { inventoryId = result.Value });

            return BadRequest(new { error = result.Error });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Pharmacist")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateInventoryCommand command)
        {
            if (id != command.Id)
                return BadRequest(new { error = AppConstants.Messages.IdMismatch });

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(new { message = AppConstants.Messages.InventoryUpdated });

            if (result.Error!.Contains(AppConstants.Messages.NotFoundKeyword))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppConstants.Roles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteInventoryCommand { Id = id });

            if (result.IsSuccess)
                return Ok(new { message = AppConstants.Messages.InventoryDeleted });

            if (result.Error!.Contains(AppConstants.Messages.NotFoundKeyword))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        [HttpPatch("{id}/add-stock")]
        [Authorize(Roles = "Admin,Pharmacist")]
        public async Task<IActionResult> AddStock(int id, [FromBody] AddStockCommand command)
        {
            if (id != command.InventoryId)
                return BadRequest(new { error = AppConstants.Messages.IdMismatch });

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(new { message = AppConstants.Messages.StockAdded });

            if (result.Error!.Contains(AppConstants.Messages.NotFoundKeyword))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        [HttpPatch("{id}/remove-stock")]
        [Authorize(Roles = "Admin,Pharmacist")]
        public async Task<IActionResult> RemoveStock(int id, [FromBody] RemoveStockCommand command)
        {
            if (id != command.InventoryId)
                return BadRequest(new { error = AppConstants.Messages.IdMismatch });

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(new { message = AppConstants.Messages.StockRemoved });

            if (result.Error!.Contains(AppConstants.Messages.NotFoundKeyword))
                return NotFound(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }
    }
}