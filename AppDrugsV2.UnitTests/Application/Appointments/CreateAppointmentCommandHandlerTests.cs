using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Appointments.Commands;
using AppDrugsV2.Application.Features.Appointments.DTOs;
using AppDrugsV2.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppDrugsV2.UnitTests.Application.Appointments
{
    [TestFixture]
    public class CreateAppointmentCommandHandlerTests
    {
        private Mock<IApplicationDbContext> _contextMock;
        private Mock<ICurrentUserService> _currentUserServiceMock;
        private CreateAppointmentCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _handler = new CreateAppointmentCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
        }

        [Test]
        public async Task Handle_WhenUserNotAuthenticated_ShouldReturnFailure()
        {
            _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);
            var command = new CreateAppointmentCommand { GestorFarmaceuticoId = 1 };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(AppConstants.Messages.UserNotAuthenticated);
        }

        [Test]
        public async Task Handle_WhenGestorDoesNotExist_ShouldReturnFailure()
        {
            _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(x => x.UserId).Returns(1);

            var gestoresDbSet = new List<GestorFarmaceutico>().AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.GestoresFarmaceuticos).Returns(gestoresDbSet.Object);

            var command = new CreateAppointmentCommand { GestorFarmaceuticoId = 1 };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().BeEquivalentTo(string.Format(AppConstants.Messages.SedeNotExists, command.GestorFarmaceuticoId));
        }

        [Test]
        public async Task Handle_WhenInventoryDoesNotExist_ShouldReturnFailure()
        {
            _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(x => x.UserId).Returns(1);

            var gestor = new GestorFarmaceutico("Sede Norte", "Direccion", "123", 1);
            typeof(GestorFarmaceutico).GetProperty("Id")!.SetValue(gestor, 1);

            var gestoresDbSet = new List<GestorFarmaceutico> { gestor }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.GestoresFarmaceuticos).Returns(gestoresDbSet.Object);

           
            var inventoriesDbSet = new List<Inventory>().AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Inventories).Returns(inventoriesDbSet.Object);
            
            
            _contextMock.Setup(c => c.Appointments.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
                        .Returns(new ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Appointment>>());

            var command = new CreateAppointmentCommand 
            { 
                GestorFarmaceuticoId = 1,
                Details = new List<CreateAppointmentDetailCommand> { new CreateAppointmentDetailCommand { InventoryId = 10, Quantity = 5 } }
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().BeEquivalentTo(string.Format(AppConstants.Messages.InventoryNotExists, 10));
        }

        [Test]
        public async Task Handle_WhenInsufficientStock_ShouldReturnFailure()
        {
            _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(x => x.UserId).Returns(1);

            var gestor = new GestorFarmaceutico("Sede Norte", "Direccion", "123", 1);
            typeof(GestorFarmaceutico).GetProperty("Id")!.SetValue(gestor, 1);
            var gestoresDbSet = new List<GestorFarmaceutico> { gestor }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.GestoresFarmaceuticos).Returns(gestoresDbSet.Object);

            var inventory = new Inventory(1, 1, 2); 
            typeof(Inventory).GetProperty("Id")!.SetValue(inventory, 10);
            var inventoriesDbSet = new List<Inventory> { inventory }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Inventories).Returns(inventoriesDbSet.Object);

            _contextMock.Setup(c => c.Appointments.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
                        .Returns(new ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Appointment>>());

            var command = new CreateAppointmentCommand 
            { 
                GestorFarmaceuticoId = 1,
                Details = new List<CreateAppointmentDetailCommand> { new CreateAppointmentDetailCommand { InventoryId = 10, Quantity = 5 } }
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().BeEquivalentTo(string.Format(AppConstants.Messages.InsufficientStock, 2));
        }

        [Test]
        public async Task Handle_WhenValidRequest_ShouldCreateAppointmentDeductStockAndReturnSuccess()
        {
            _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(x => x.UserId).Returns(1);

            var gestor = new GestorFarmaceutico("Sede Norte", "Direccion", "123", 1);
            typeof(GestorFarmaceutico).GetProperty("Id")!.SetValue(gestor, 1);
            var gestoresDbSet = new List<GestorFarmaceutico> { gestor }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.GestoresFarmaceuticos).Returns(gestoresDbSet.Object);

            var inventory = new Inventory(1, 1, 10);
            typeof(Inventory).GetProperty("Id")!.SetValue(inventory, 10);
            var inventoriesDbSet = new List<Inventory> { inventory }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Inventories).Returns(inventoriesDbSet.Object);

            _contextMock.Setup(c => c.Appointments.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
                        .Callback<Appointment, CancellationToken>((app, ct) => typeof(Appointment).GetProperty("Id")!.SetValue(app, 99))
                        .Returns(new ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Appointment>>());

            _contextMock.Setup(c => c.Notifications.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
                        .Returns(new ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Notification>>());

            var command = new CreateAppointmentCommand 
            { 
                GestorFarmaceuticoId = 1,
                Details = new List<CreateAppointmentDetailCommand> { new CreateAppointmentDetailCommand { InventoryId = 10, Quantity = 5 } }
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                result.IsSuccess.Should().BeTrue();
                result.Value.Should().Be(99);
                inventory.Quantity.Should().Be(5); 
                
                _contextMock.Verify(c => c.Appointments.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
                _contextMock.Verify(c => c.Notifications.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
                _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
            });
        }
    }
}
