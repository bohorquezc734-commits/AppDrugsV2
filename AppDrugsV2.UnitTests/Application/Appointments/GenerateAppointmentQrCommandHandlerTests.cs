using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Appointments.Commands;
using AppDrugsV2.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppDrugsV2.UnitTests.Application.Appointments
{
    [TestFixture]
    public class GenerateAppointmentQrCommandHandlerTests
    {
        private Mock<IApplicationDbContext> _contextMock;
        private Mock<IQrCodeService> _qrCodeServiceMock;
        private Mock<INotificationHubService> _notificationHubMock;
        private GenerateAppointmentQrCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _qrCodeServiceMock = new Mock<IQrCodeService>();
            _notificationHubMock = new Mock<INotificationHubService>();
            _handler = new GenerateAppointmentQrCommandHandler(
                _contextMock.Object,
                _qrCodeServiceMock.Object,
                _notificationHubMock.Object);
        }

        [Test]
        public async Task Handle_WhenAppointmentDoesNotExist_ShouldReturnFailure()
        {
            
            var appointmentsDbSet = new List<Appointment>().AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Appointments).Returns(appointmentsDbSet.Object);

            var command = new GenerateAppointmentQrCommand { AppointmentId = 1 };

           
            var result = await _handler.Handle(command, CancellationToken.None);

            
            Assert.Multiple(() =>
            {
                result.IsSuccess.Should().BeFalse();
                result.Error.Should().Contain(AppConstants.Messages.NotExistsKeyword);
            });
        }

        [Test]
        public async Task Handle_WhenValidRequest_ShouldAssignQrCodeAndReturnSuccess()
        {
            
            var appointment = new Appointment(1, 2);
            typeof(Appointment).GetProperty("Id")!.SetValue(appointment, 1);
            
            var appointmentsDbSet = new List<Appointment> { appointment }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Appointments).Returns(appointmentsDbSet.Object);

            string expectedQr = "fake_base64_qr_code";
            _qrCodeServiceMock.Setup(q => q.GenerateBase64(It.IsAny<string>())).Returns(expectedQr);

            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _notificationHubMock.Setup(n => n.SendToUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new GenerateAppointmentQrCommand { AppointmentId = 1 };

           
            var result = await _handler.Handle(command, CancellationToken.None);

            
            Assert.Multiple(() =>
            {
                result.IsSuccess.Should().BeTrue();
                result.Value.Should().Be(expectedQr);

                _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
                _notificationHubMock.Verify(n => n.SendToUserAsync(1, "QrReady", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
            });
        }

        [Test]
        public async Task Handle_WhenNotificationFails_ShouldStillReturnSuccessAndPersist()
        {
           
            var appointment = new Appointment(1, 2);
            typeof(Appointment).GetProperty("Id")!.SetValue(appointment, 1);

            var appointmentsDbSet = new List<Appointment> { appointment }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Appointments).Returns(appointmentsDbSet.Object);

            string expectedQr = "fake_base64_qr_code";
            _qrCodeServiceMock.Setup(q => q.GenerateBase64(It.IsAny<string>())).Returns(expectedQr);

            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

  
            _notificationHubMock.Setup(n => n.SendToUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Hub error"));

            var command = new GenerateAppointmentQrCommand { AppointmentId = 1 };

            
            var result = await _handler.Handle(command, CancellationToken.None);

            
            Assert.Multiple(() =>
            {
                result.IsSuccess.Should().BeTrue();
                result.Value.Should().Be(expectedQr);

                _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            });
        }

        [Test]
        public async Task Handle_WhenAssignQrCodeThrowsArgumentException_ShouldReturnFailure()
        {
          
            var appointment = new Appointment(1, 2);
            typeof(Appointment).GetProperty("Id")!.SetValue(appointment, 1);

            var appointmentsDbSet = new List<Appointment> { appointment }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Appointments).Returns(appointmentsDbSet.Object);

          
            _qrCodeServiceMock.Setup(q => q.GenerateBase64(It.IsAny<string>())).Returns(string.Empty);

            var command = new GenerateAppointmentQrCommand { AppointmentId = 1 };

            
            var result = await _handler.Handle(command, CancellationToken.None);

           
            Assert.Multiple(() =>
            {
                result.IsSuccess.Should().BeFalse();
                _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            });
        }
    }
}
