using AppDrugsV2.Api.Controllers;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Features.Appointments.Commands;
using AppDrugsV2.Application.Features.Appointments.DTOs;
using AppDrugsV2.Application.Features.Appointments.Queries;
using AppDrugsV2.Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace AppDrugsV2.UnitTests.Controllers
{
    [TestFixture]
    public class AppointmentsControllerTests
    {
        private Mock<IMediator> _mediatorMock;
        private AppointmentsController _controller;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new AppointmentsController(_mediatorMock.Object);
        }

        private void SetupUserClaims(int userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task GetAll_ShouldReturnOkResult_WithPagedResult()
        {
            var query = new ListAppointmentsQuery();
            var pagedResult = new PagedResult<AppointmentDto>(new List<AppointmentDto>(), 0, 1, 10);
            _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>())).ReturnsAsync(pagedResult);

           
            var result = await _controller.GetAll(query);

           
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(pagedResult);
        }

        [Test]
        public async Task GetMyAppointments_ShouldReturnOkResult_WhenUserIsAuthenticated()
        {
            var userId = 1;
            SetupUserClaims(userId);
            var query = new ListAppointmentsQuery();
            var pagedResult = new PagedResult<AppointmentDto>(new List<AppointmentDto>(), 0, 1, 10);
            
            _mediatorMock.Setup(m => m.Send(It.Is<ListAppointmentsQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            var result = await _controller.GetMyAppointments(query);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(pagedResult);
        }

        [Test]
        public async Task GetMyAppointments_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var query = new ListAppointmentsQuery();

            var result = await _controller.GetMyAppointments(query);

            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Test]
        public async Task GetById_ShouldReturnOkResult_WhenAppointmentExists()
        {
            var appointmentId = 1;
            var expectedDto = new AppointmentDto { Id = appointmentId };
            _mediatorMock.Setup(m => m.Send(It.Is<GetAppointmentQuery>(q => q.Id == appointmentId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.GetById(appointmentId);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedDto);
        }

        [Test]
        public async Task GetById_ShouldReturnNotFound_WhenKeyNotFoundExceptionThrown()
        {
            
            var appointmentId = 1;
            var exceptionMessage = "Not found";
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAppointmentQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException(exceptionMessage));

            var result = await _controller.GetById(appointmentId);

            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().NotBeNull();
        }

        [Test]
        public async Task Create_ShouldReturnCreatedAtAction_WhenSuccessful()
        {
            var command = new CreateAppointmentCommand();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Content-Type"] = "multipart/form-data";
            httpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), new FormFileCollection());

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Success(1));

            var result = await _controller.Create(command);

            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(_controller.GetById));
            createdResult.RouteValues!["id"].Should().Be(1);
        }

        [Test]
        public async Task Create_ShouldReturnBadRequest_WhenFailure()
        {
            var command = new CreateAppointmentCommand();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Content-Type"] = "multipart/form-data";
            httpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), new FormFileCollection());

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            var errorMsg = "Creation failed";
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Failure(errorMsg));

            var result = await _controller.Create(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task UpdateStatus_ShouldReturnOk_WhenSuccessful()
        {
            var appointmentId = 1;
            var command = new UpdateAppointmentStatusCommand { AppointmentId = appointmentId };
            
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.UpdateStatus(appointmentId, command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task UpdateStatus_ShouldReturnBadRequest_WhenIdMismatch()
        {
            var appointmentId = 1;
            var command = new UpdateAppointmentStatusCommand { AppointmentId = 2 };

            var result = await _controller.UpdateStatus(appointmentId, command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task UpdateStatus_ShouldReturnNotFound_WhenNotExists()
        {
            var appointmentId = 1;
            var command = new UpdateAppointmentStatusCommand { AppointmentId = appointmentId };
            
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure(AppConstants.Messages.NotExistsKeyword));

            
            var result = await _controller.UpdateStatus(appointmentId, command);

            
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task GenerateQr_ShouldReturnOk_WhenSuccessful()
        {
            
            var appointmentId = 1;
            var expectedQr = "base64qr";
            _mediatorMock.Setup(m => m.Send(It.Is<GenerateAppointmentQrCommand>(c => c.AppointmentId == appointmentId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<string>.Success(expectedQr));

            var result = await _controller.GenerateQr(appointmentId);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task DownloadFile_ShouldReturnFile_WhenSuccessful()
        {
            var appointmentId = 1;
            var dto = new FileResultDto
            {
                FileName = "test.pdf",
                ContentType = "application/pdf",
                Content = new byte[] { 1, 2, 3 }
            };
            
            _mediatorMock.Setup(m => m.Send(It.Is<DownloadAppointmentFileQuery>(q => q.AppointmentId == appointmentId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<FileResultDto>.Success(dto));

            var result = await _controller.DownloadFile(appointmentId);

            result.Should().BeOfType<FileContentResult>();
            var fileResult = result as FileContentResult;
            fileResult!.FileContents.Should().BeEquivalentTo(dto.Content);
            fileResult.ContentType.Should().Be(dto.ContentType);
            fileResult.FileDownloadName.Should().Be(dto.FileName);
        }

        [Test]
        public async Task DownloadFile_ShouldReturnNotFound_WhenNotExists()
        {
            var appointmentId = 1;
            _mediatorMock.Setup(m => m.Send(It.IsAny<DownloadAppointmentFileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<FileResultDto>.Failure(AppConstants.Messages.NotExistsKeyword));

            var result = await _controller.DownloadFile(appointmentId);

           
            result.Should().BeOfType<NotFoundObjectResult>();
        }
        [Test]
        public async Task GetMyAppointments_ShouldReturnUnauthorized_WhenUserIdIsInvalid()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "invalid-id")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            var query = new ListAppointmentsQuery();

            var result = await _controller.GetMyAppointments(query);

            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Test]
        public async Task Create_ShouldAssignFileProperties_WhenFileIsUploaded()
        {
            
            var command = default(CreateAppointmentCommand);
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateAppointmentCommand>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<Result<int>>, CancellationToken>((c, ct) => command = (CreateAppointmentCommand)c)
                .ReturnsAsync(Result<int>.Success(1));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Content-Type"] = "multipart/form-data";
            
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            fileMock.Setup(f => f.ContentType).Returns("text/plain");
            var ms = new System.IO.MemoryStream(new byte[] { 1, 2, 3 });
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<System.IO.Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback<System.IO.Stream, CancellationToken>((s, _) => ms.CopyTo(s)); // Simplified mock

            var formFileCollection = new FormFileCollection { fileMock.Object };
            httpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), formFileCollection);

            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var inputCommand = new CreateAppointmentCommand();
            var result = await _controller.Create(inputCommand);

            result.Should().BeOfType<CreatedAtActionResult>();
            command.Should().NotBeNull();
            command.ArchivoNombre.Should().Be("test.txt");
            command.ArchivoContentType.Should().Be("text/plain");
        }

        [Test]
        public async Task UpdateStatus_ShouldReturnBadRequest_WhenOtherFailure()
        {
            var appointmentId = 1;
            var command = new UpdateAppointmentStatusCommand { AppointmentId = appointmentId };
            
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("General error"));

            var result = await _controller.UpdateStatus(appointmentId, command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GenerateQr_ShouldReturnNotFound_WhenNotExists()
        {
            var appointmentId = 1;
            _mediatorMock.Setup(m => m.Send(It.Is<GenerateAppointmentQrCommand>(c => c.AppointmentId == appointmentId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<string>.Failure(AppConstants.Messages.NotExistsKeyword));

            var result = await _controller.GenerateQr(appointmentId);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task GenerateQr_ShouldReturnBadRequest_WhenOtherFailure()
        {
            var appointmentId = 1;
            _mediatorMock.Setup(m => m.Send(It.Is<GenerateAppointmentQrCommand>(c => c.AppointmentId == appointmentId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<string>.Failure("General error"));

            var result = await _controller.GenerateQr(appointmentId);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task DownloadFile_ShouldReturnBadRequest_WhenOtherFailure()
        {
            var appointmentId = 1;
            _mediatorMock.Setup(m => m.Send(It.IsAny<DownloadAppointmentFileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<FileResultDto>.Failure("General error"));

            var result = await _controller.DownloadFile(appointmentId);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task DownloadFile_ShouldReturnInternalServerError_WhenExceptionThrown()
        {
            var appointmentId = 1;
            _mediatorMock.Setup(m => m.Send(It.IsAny<DownloadAppointmentFileQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.Exception("Critical failure"));

            var result = await _controller.DownloadFile(appointmentId);

            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult!.StatusCode.Should().Be(500);
        }
    }
}
