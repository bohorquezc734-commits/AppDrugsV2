using AppDrugsV2.Api.Controllers;
using AppDrugsV2.Application.Common.Constants;
using AppDrugsV2.Application.Common.Results;
using AppDrugsV2.Application.Features.Auth.Commands;
using AppDrugsV2.Application.Features.Auth.DTOs;
using AppDrugsV2.Application.Features.Auth.Queries;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace AppDrugsV2.UnitTests.Controllers
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<IMediator> _mediatorMock;
        private AuthController _controller;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new AuthController(_mediatorMock.Object);
        }

        private void SetupUserClaims(int? userId)
        {
            var claims = new List<Claim>();
            if (userId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, "invalid"));
            }

            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task Register_ShouldReturnOk_WhenSuccessful()
        {
            var command = new RegisterUserCommand();
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Success(1));

            var result = await _controller.Register(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task Register_ShouldReturnBadRequest_WhenFailure()
        {
            var command = new RegisterUserCommand();
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Failure("Error"));

            var result = await _controller.Register(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Login_ShouldReturnOk_AndSetCookie_WhenSuccessful()
        {
            var query = new LoginQuery();
            var response = new LoginResponse 
            { 
                Token = "token", 
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                UserId = 1,
                FullName = "Test",
                Role = "Admin",
                RefreshToken = "ref",
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(1)
            };
            
            _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<LoginResponse>.Success(response));

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await _controller.Login(query);

            result.Should().BeOfType<OkObjectResult>();
            var setCookieHeader = httpContext.Response.Headers["Set-Cookie"].ToString();
            setCookieHeader.Should().Contain("X-Access-Token=token");
        }

        [Test]
        public async Task Login_ShouldReturnUnauthorized_WhenFailure()
        {
            var query = new LoginQuery();
            _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<LoginResponse>.Failure("Error"));

            var result = await _controller.Login(query);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Test]
        public void Logout_ShouldReturnOk_AndClearCookie_WhenSuccessful()
        {
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = _controller.Logout();

            result.Should().BeOfType<OkObjectResult>();
            var setCookieHeader = httpContext.Response.Headers["Set-Cookie"].ToString();
            setCookieHeader.Should().Contain("X-Access-Token=;");
            setCookieHeader.Should().Contain("expires=");
        }

        [Test]
        public async Task ForgotPassword_ShouldReturnOk()
        {
            var command = new ForgotPasswordCommand();
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.ForgotPassword(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task ResetPassword_ShouldReturnOk_WhenSuccessful()
        {
            var command = new ResetPasswordCommand();
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.ResetPassword(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task ResetPassword_ShouldReturnBadRequest_WhenFailure()
        {
            var command = new ResetPasswordCommand();
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("Error"));

            var result = await _controller.ResetPassword(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task ChangePassword_ShouldReturnOk_WhenSuccessful()
        {
            SetupUserClaims(1);
            var command = new ChangePasswordCommand();
            _mediatorMock.Setup(m => m.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.ChangePassword(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task ChangePassword_ShouldReturnBadRequest_WhenFailure()
        {
            SetupUserClaims(1);
            var command = new ChangePasswordCommand();
            _mediatorMock.Setup(m => m.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("Error"));

            var result = await _controller.ChangePassword(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task ChangePassword_ShouldReturnUnauthorized_WhenUserInvalid()
        {
            SetupUserClaims(null); // Invalid parse
            var command = new ChangePasswordCommand();

            var result = await _controller.ChangePassword(command);

            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Test]
        public async Task UpdateProfile_ShouldReturnOk_WhenSuccessful()
        {
            SetupUserClaims(1);
            var command = new UpdateProfileCommand();
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateProfileCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.UpdateProfile(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task UpdateProfile_ShouldReturnBadRequest_WhenFailure()
        {
            SetupUserClaims(1);
            var command = new UpdateProfileCommand();
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateProfileCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("Error"));

            var result = await _controller.UpdateProfile(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task UpdateProfile_ShouldReturnUnauthorized_WhenUserInvalid()
        {
            SetupUserClaims(null); 
            var command = new UpdateProfileCommand();

            var result = await _controller.UpdateProfile(command);

            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Test]
        public async Task GetAllUsers_ShouldReturnOk_WhenSuccessful()
        {
            var query = new GetAllUsersQuery();
            var pagedResult = new PagedResult<UserDto>(new List<UserDto>(), 0, 1, 20);
            _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<PagedResult<UserDto>>.Success(pagedResult));

            var result = await _controller.GetAllUsers(query);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task GetAllUsers_ShouldReturnBadRequest_WhenFailure()
        {
            var query = new GetAllUsersQuery();
            _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<PagedResult<UserDto>>.Failure("Error"));

            var result = await _controller.GetAllUsers(query);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task ChangeUserRole_ShouldReturnOk_WhenSuccessful()
        {
            var command = new ChangeUserRoleCommand { UserId = 1 };
            _mediatorMock.Setup(m => m.Send(It.IsAny<ChangeUserRoleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.ChangeUserRole(1, command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task ChangeUserRole_ShouldReturnNotFound_WhenNotExists()
        {
            var command = new ChangeUserRoleCommand { UserId = 1 };
            _mediatorMock.Setup(m => m.Send(It.IsAny<ChangeUserRoleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure(AppConstants.Messages.NotFoundKeyword));

            var result = await _controller.ChangeUserRole(1, command);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task ChangeUserRole_ShouldReturnBadRequest_WhenFailure()
        {
            var command = new ChangeUserRoleCommand { UserId = 1 };
            _mediatorMock.Setup(m => m.Send(It.IsAny<ChangeUserRoleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("Error"));

            var result = await _controller.ChangeUserRole(1, command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task ToggleUserStatus_ShouldReturnOk_WhenSuccessful()
        {
            var command = new ToggleUserStatusCommand { UserId = 1 };
            _mediatorMock.Setup(m => m.Send(It.IsAny<ToggleUserStatusCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.ToggleUserStatus(1, command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task ToggleUserStatus_ShouldReturnNotFound_WhenNotExists()
        {
            var command = new ToggleUserStatusCommand { UserId = 1 };
            _mediatorMock.Setup(m => m.Send(It.IsAny<ToggleUserStatusCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure(AppConstants.Messages.NotFoundKeyword));

            var result = await _controller.ToggleUserStatus(1, command);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task ToggleUserStatus_ShouldReturnBadRequest_WhenFailure()
        {
            var command = new ToggleUserStatusCommand { UserId = 1 };
            _mediatorMock.Setup(m => m.Send(It.IsAny<ToggleUserStatusCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("Error"));

            var result = await _controller.ToggleUserStatus(1, command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task Refresh_ShouldReturnOk_WhenSuccessful()
        {
            var command = new RefreshTokenCommand();
            var response = new RefreshTokenResponse { Token = "newToken" };
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<RefreshTokenResponse>.Success(response));

            var result = await _controller.Refresh(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task Refresh_ShouldReturnUnauthorized_WhenFailure()
        {
            var command = new RefreshTokenCommand();
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<RefreshTokenResponse>.Failure("Error"));

            var result = await _controller.Refresh(command);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }
    }
}
