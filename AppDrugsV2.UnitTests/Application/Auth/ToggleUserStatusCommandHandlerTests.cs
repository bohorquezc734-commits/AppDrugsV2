using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Common.Interfaces;
using AppDrugsV2.Application.Features.Auth.Commands;
using AppDrugsV2.Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppDrugsV2.UnitTests.Application.Auth
{
    [TestFixture]
    public class ToggleUserStatusCommandHandlerTests
    {
        private Mock<IApplicationDbContext> _contextMock;
        private ToggleUserStatusCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new ToggleUserStatusCommandHandler(_contextMock.Object);
        }

        [Test]
        public async Task Handle_WhenUserDoesNotExist_ShouldReturnFailure()
        {
          
            var emptyList = new List<User>().AsQueryable().BuildMockDbSet();
            
            emptyList.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((User?)null);
            
            _contextMock.Setup(c => c.Users).Returns(emptyList.Object);

            var command = new ToggleUserStatusCommand { UserId = 99, IsActive = false };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain(AppConstants.Messages.NotFoundKeyword);
        }

        [Test]
        public async Task Handle_WhenUserExistsAndCommandIsActive_ShouldActivateUser()
        {
            var user = new User("test@test.com", "hash", "John Doe");
            user.Deactivate(); 
            
            var userList = new List<User> { user }.AsQueryable().BuildMockDbSet();
            userList.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(user);

            _contextMock.Setup(c => c.Users).Returns(userList.Object);

            var command = new ToggleUserStatusCommand { UserId = 1, IsActive = true };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            user.IsActive.Should().BeTrue();
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
