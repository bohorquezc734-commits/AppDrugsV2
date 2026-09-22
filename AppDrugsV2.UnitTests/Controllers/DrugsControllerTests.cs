using AppDrugsV2.Api.Controllers;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Features.Drugs.Commands;
using AppDrugsV2.Application.Features.Drugs.DTOs;
using AppDrugsV2.Application.Features.Drugs.Queries;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppDrugsV2.UnitTests.Controllers
{
    [TestFixture]
    public class DrugsControllerTests
    {
        private Mock<IMediator> _mediatorMock;
        private DrugsController _controller;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new DrugsController(_mediatorMock.Object);
        }

        [Test]
        public async Task GetAll_ShouldReturnOk_WhenSuccessful()
        {
            var query = new ListDrugsQuery();
            var pagedResult = new PagedResult<DrugDto>(new List<DrugDto>(), 0, 1, 10);
            
            _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            var result = await _controller.GetAll(query);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(pagedResult);
        }

        [Test]
        public async Task GetById_ShouldReturnOk_WhenSuccessful()
        {
            var dto = new DrugDto { Id = 1 };
            _mediatorMock.Setup(m => m.Send(It.Is<GetDrugQuery>(q => q.Id == 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await _controller.GetById(1);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(dto);
        }

        [Test]
        public async Task GetById_ShouldReturnNotFound_WhenKeyNotFoundExceptionThrown()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetDrugQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Not found"));

            var result = await _controller.GetById(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task Create_ShouldReturnCreatedAtAction_WhenSuccessful()
        {
            var command = new CreateDrugCommand();
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Success(1));

            var result = await _controller.Create(command);

            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(DrugsController.GetById));
            createdResult.RouteValues!["id"].Should().Be(1);
        }

        [Test]
        public async Task Create_ShouldReturnBadRequest_WhenFailure()
        {
            var command = new CreateDrugCommand();
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Failure("Error"));

            var result = await _controller.Create(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Update_ShouldReturnOk_WhenSuccessful()
        {
            var command = new UpdateDrugCommand { Id = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.Update(1, command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task Update_ShouldReturnBadRequest_WhenIdMismatch()
        {
            var command = new UpdateDrugCommand { Id = 2 };

            var result = await _controller.Update(1, command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Update_ShouldReturnNotFound_WhenNotExists()
        {
            var command = new UpdateDrugCommand { Id = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure(AppConstants.Messages.NotFoundKeyword));

            var result = await _controller.Update(1, command);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task Update_ShouldReturnBadRequest_WhenOtherFailure()
        {
            var command = new UpdateDrugCommand { Id = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("General error"));

            var result = await _controller.Update(1, command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Delete_ShouldReturnOk_WhenSuccessful()
        {
            _mediatorMock.Setup(m => m.Send(It.Is<DeleteDrugCommand>(c => c.Id == 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task Delete_ShouldReturnNotFound_WhenNotExists()
        {
            _mediatorMock.Setup(m => m.Send(It.Is<DeleteDrugCommand>(c => c.Id == 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure(AppConstants.Messages.NotFoundKeyword));

            var result = await _controller.Delete(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task Delete_ShouldReturnBadRequest_WhenOtherFailure()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteDrugCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("General error"));

            var result = await _controller.Delete(1);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task UpdateStock_ShouldReturnOk_WhenSuccessful()
        {
            var command = new UpdateStockCommand { DrugId = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.UpdateStock(1, command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task UpdateStock_ShouldReturnBadRequest_WhenIdMismatch()
        {
            var command = new UpdateStockCommand { DrugId = 2 };

            var result = await _controller.UpdateStock(1, command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task UpdateStock_ShouldReturnNotFound_WhenNotExists()
        {
            var command = new UpdateStockCommand { DrugId = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure(AppConstants.Messages.NotFoundKeyword));

            var result = await _controller.UpdateStock(1, command);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task UpdateStock_ShouldReturnBadRequest_WhenOtherFailure()
        {
            var command = new UpdateStockCommand { DrugId = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("General error"));

            var result = await _controller.UpdateStock(1, command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
