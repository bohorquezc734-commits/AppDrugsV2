using AppDrugsV2.Api.Controllers;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Features.Gestores.Commands;
using AppDrugsV2.Application.Features.Gestores.DTOs;
using AppDrugsV2.Application.Features.Gestores.Queries;
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
    public class GestoresControllerTests
    {
        private Mock<IMediator> _mediatorMock;
        private GestoresController _controller;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new GestoresController(_mediatorMock.Object);
        }

        [Test]
        public async Task GetAll_ShouldReturnOk_WhenSuccessful()
        {
            var query = new ListGestoresQuery();
            var dtos = new List<GestorDto>();
            
            _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dtos);

            var result = await _controller.GetAll(query);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(dtos);
        }

        [Test]
        public async Task GetById_ShouldReturnOk_WhenSuccessful()
        {
            var dto = new GestorDto { Id = 1 };
            _mediatorMock.Setup(m => m.Send(It.Is<GetGestorQuery>(q => q.Id == 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await _controller.GetById(1);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(dto);
        }

        [Test]
        public async Task GetById_ShouldReturnNotFound_WhenKeyNotFoundExceptionThrown()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetGestorQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Not found"));

            var result = await _controller.GetById(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task Create_ShouldReturnCreatedAtAction_WhenSuccessful()
        {
            var command = new CreateGestorCommand();
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Success(1));

            var result = await _controller.Create(command);

            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Test]
        public async Task Create_ShouldReturnBadRequest_WhenFailure()
        {
            var command = new CreateGestorCommand();
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Failure("Error"));

            var result = await _controller.Create(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Update_ShouldReturnOk_WhenSuccessful()
        {
            var command = new UpdateGestorCommand { Id = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Success(1));

            var result = await _controller.Update(1, command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task Update_ShouldReturnBadRequest_WhenIdMismatch()
        {
            var command = new UpdateGestorCommand { Id = 2 };

            var result = await _controller.Update(1, command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Update_ShouldReturnNotFound_WhenNotExists()
        {
            var command = new UpdateGestorCommand { Id = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Failure(AppConstants.Messages.NotFoundKeyword));

            var result = await _controller.Update(1, command);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task Update_ShouldReturnBadRequest_WhenOtherFailure()
        {
            var command = new UpdateGestorCommand { Id = 1 };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Failure("General error"));

            var result = await _controller.Update(1, command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Delete_ShouldReturnOk_WhenSuccessful()
        {
            _mediatorMock.Setup(m => m.Send(It.Is<DeleteGestorCommand>(c => c.Id == 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task Delete_ShouldReturnNotFound_WhenNotExists()
        {
            _mediatorMock.Setup(m => m.Send(It.Is<DeleteGestorCommand>(c => c.Id == 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure(AppConstants.Messages.NotFoundKeyword));

            var result = await _controller.Delete(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task Delete_ShouldReturnBadRequest_WhenOtherFailure()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteGestorCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("General error"));

            var result = await _controller.Delete(1);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
