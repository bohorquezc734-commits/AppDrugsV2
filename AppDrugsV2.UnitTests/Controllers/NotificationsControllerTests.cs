using AppDrugsV2.Api.Controllers;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Features.Notifications.Commands;

using AppDrugsV2.Application.Features.Notifications.Queries;
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
    public class NotificationsControllerTests
    {
        private Mock<IMediator> _mediatorMock;
        private NotificationsController _controller;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new NotificationsController(_mediatorMock.Object);
        }

        [Test]
        public async Task GetMyNotifications_ShouldReturnOk_WhenSuccessful()
        {
            var dtos = new List<NotificationDto>();
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetMyNotificationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<NotificationDto>>.Success(dtos));

            var result = await _controller.GetMyNotifications();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task GetMyNotifications_ShouldReturnBadRequest_WhenFailure()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetMyNotificationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<NotificationDto>>.Failure("Error"));

            var result = await _controller.GetMyNotifications();

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task MarkAsRead_ShouldReturnOk_WhenSuccessful()
        {
            _mediatorMock.Setup(m => m.Send(It.Is<MarkAsReadCommand>(c => c.Id == 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.MarkAsRead(1);

            result.Should().BeOfType<OkResult>();
        }

        [Test]
        public async Task MarkAsRead_ShouldReturnBadRequest_WhenFailure()
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<MarkAsReadCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("Error"));

            var result = await _controller.MarkAsRead(1);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
