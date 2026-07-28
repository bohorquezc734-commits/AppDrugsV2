using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Appointments.Commands;
using AppDrugsV2.Domain.Entities;
using AppDrugsV2.Domain.Enums;
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
    public class UpdateAppointmentStatusCommandHandlerTests
    {
        private Mock<IApplicationDbContext> _contextMock;
        private Mock<ICurrentUserService> _currentUserServiceMock;
        private UpdateAppointmentStatusCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _handler = new UpdateAppointmentStatusCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
        }

        [Test]
        public async Task Handle_WhenUserNotAuthenticated_ShouldReturnFailure()
        {
            _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);
            var command = new UpdateAppointmentStatusCommand { AppointmentId = 1, NewStatus = AppointmentStatus.Entregado };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(AppConstants.Messages.UserNotAuthenticated);
        }

        [Test]
        public async Task Handle_WhenAppointmentDoesNotExist_ShouldReturnFailure()
        {
           
            _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
            var emptyList = new List<Appointment>().AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Appointments).Returns(emptyList.Object);

            var command = new UpdateAppointmentStatusCommand { AppointmentId = 1, NewStatus = AppointmentStatus.Entregado };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain(AppConstants.Messages.NotExistsKeyword);
        }

        [Test]
        public async Task Handle_WhenStatusIsEntregado_ShouldDeductStockAndReturnSuccess()
        {
            _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);

            var appointment = new Appointment(1, 2);
            var detail = new AppointmentDetail(1, 1, 10);
            typeof(AppointmentDetail).GetProperty("Id")!.SetValue(detail, 1);
            appointment.AddDetail(detail);
            appointment.Confirmar(); 
            typeof(Appointment).GetProperty("Id")!.SetValue(appointment, 1);

            var inventory = new Inventory(1, 2, 50); 
            typeof(Inventory).GetProperty("Id")!.SetValue(inventory, 1);

            var appointmentsDbSet = new List<Appointment> { appointment }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Appointments).Returns(appointmentsDbSet.Object);

            var inventoriesDbSet = new List<Inventory> { inventory }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Inventories).Returns(inventoriesDbSet.Object);
            
            var notificationsDbSet = new List<Notification>().AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Notifications).Returns(notificationsDbSet.Object);

            var command = new UpdateAppointmentStatusCommand { AppointmentId = 1, NewStatus = AppointmentStatus.Entregado };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            appointment.Status.Should().Be(AppointmentStatus.Entregado);
            inventory.Quantity.Should().Be(40); 
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_WhenInsufficientStock_ShouldReturnFailure()
        {
            _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);

            var appointment = new Appointment(1, 2);
            var detail = new AppointmentDetail(1, 1, 100); 
            appointment.AddDetail(detail);
            appointment.Confirmar();
            typeof(Appointment).GetProperty("Id")!.SetValue(appointment, 1);

            var inventory = new Inventory(1, 2, 50); 
            typeof(Inventory).GetProperty("Id")!.SetValue(inventory, 1);

            var appointmentsDbSet = new List<Appointment> { appointment }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Appointments).Returns(appointmentsDbSet.Object);

            var inventoriesDbSet = new List<Inventory> { inventory }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Inventories).Returns(inventoriesDbSet.Object);

            var command = new UpdateAppointmentStatusCommand { AppointmentId = 1, NewStatus = AppointmentStatus.Entregado };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Stock insuficiente");
            appointment.Status.Should().Be(AppointmentStatus.EnProceso); 
        }
    }
}
