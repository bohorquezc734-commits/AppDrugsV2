using AppDrugsV2.Api.Controllers;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Features.Inventories.Commands;
using AppDrugsV2.Application.Features.Inventories.DTOs;
using AppDrugsV2.Application.Features.Inventories.Queries;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppDrugsV2.UnitTests.Controllers
{
    [TestFixture]
    public class InventoriesControllerTests
    {
        private Mock<IMediator> _mediatorMock;
        private InventoriesController _controller;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new InventoriesController(_mediatorMock.Object);
        }

        [Test]
        public async Task GetAll_ShouldReturnOk_WithResult()
        {
            // Arrange
            var query = new ListInventoriesQuery();
            var dtos = new List<InventoryDto>();
            _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dtos);

            // Act
            var result = await _controller.GetAll(query);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(dtos);
        }

        [Test]
        public async Task GetById_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var dto = new InventoryDto { Id = 1 };
            _mediatorMock.Setup(m => m.Send(It.Is<GetInventoryQuery>(q => q.Id == 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(dto);
        }

        [Test]
        public async Task GetById_ShouldReturnNotFound_WhenKeyNotFoundExceptionThrown()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetInventoryQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Not found"));

            // Act
            var result = await _controller.GetById(1);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task Create_ShouldReturnCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var command = new CreateInventoryCommand();
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Success(1));

            // Act
            var result = await _controller.Create(command);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(InventoriesController.GetById));
            createdResult.RouteValues!["id"].Should().Be(1);
        }

        [Test]
        public async Task Create_ShouldReturnBadRequest_WhenFailure()
        {
            // Arrange
            var command = new CreateInventoryCommand();
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Failure("Error"));

            // Act
            var result = await _controller.Create(command);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Update_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var command = new UpdateInventoryCommand { Id = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            // Act
            var result = await _controller.Update(1, command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task Update_ShouldReturnBadRequest_WhenIdMismatch()
        {
            // Arrange
            var command = new UpdateInventoryCommand { Id = 2 };

            // Act
            var result = await _controller.Update(1, command);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Update_ShouldReturnNotFound_WhenNotExists()
        {
            // Arrange
            var command = new UpdateInventoryCommand { Id = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure(AppConstants.Messages.NotFoundKeyword));

            // Act
            var result = await _controller.Update(1, command);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task Update_ShouldReturnBadRequest_WhenOtherFailure()
        {
            // Arrange
            var command = new UpdateInventoryCommand { Id = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("General error"));

            // Act
            var result = await _controller.Update(1, command);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Delete_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.Is<DeleteInventoryCommand>(c => c.Id == 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            // Act
            var result = await _controller.Delete(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task Delete_ShouldReturnNotFound_WhenNotExists()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.Is<DeleteInventoryCommand>(c => c.Id == 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure(AppConstants.Messages.NotFoundKeyword));

            // Act
            var result = await _controller.Delete(1);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task Delete_ShouldReturnBadRequest_WhenOtherFailure()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteInventoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("General error"));

            // Act
            var result = await _controller.Delete(1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task AddStock_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var command = new AddStockCommand { InventoryId = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            // Act
            var result = await _controller.AddStock(1, command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task AddStock_ShouldReturnBadRequest_WhenIdMismatch()
        {
            // Arrange
            var command = new AddStockCommand { InventoryId = 2 };

            // Act
            var result = await _controller.AddStock(1, command);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task AddStock_ShouldReturnNotFound_WhenNotExists()
        {
            // Arrange
            var command = new AddStockCommand { InventoryId = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure(AppConstants.Messages.NotFoundKeyword));

            // Act
            var result = await _controller.AddStock(1, command);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task AddStock_ShouldReturnBadRequest_WhenOtherFailure()
        {
            // Arrange
            var command = new AddStockCommand { InventoryId = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("General error"));

            // Act
            var result = await _controller.AddStock(1, command);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task RemoveStock_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var command = new RemoveStockCommand { InventoryId = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            // Act
            var result = await _controller.RemoveStock(1, command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task RemoveStock_ShouldReturnBadRequest_WhenIdMismatch()
        {
            // Arrange
            var command = new RemoveStockCommand { InventoryId = 2 };

            // Act
            var result = await _controller.RemoveStock(1, command);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task RemoveStock_ShouldReturnNotFound_WhenNotExists()
        {
            // Arrange
            var command = new RemoveStockCommand { InventoryId = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure(AppConstants.Messages.NotFoundKeyword));

            // Act
            var result = await _controller.RemoveStock(1, command);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task RemoveStock_ShouldReturnBadRequest_WhenOtherFailure()
        {
            // Arrange
            var command = new RemoveStockCommand { InventoryId = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("General error"));

            // Act
            var result = await _controller.RemoveStock(1, command);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
