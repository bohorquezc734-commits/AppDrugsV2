using AppDrugsV2.Domain.Entities;
using AppDrugsV2.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace AppDrugsV2.UnitTests.Domain
{
    [TestFixture]
    public class UserTests
    {
        [Test]
        public void Constructor_WithValidArguments_ShouldCreateUser()
        {
            // Arrange
            var email = "test@test.com";
            var passwordHash = "hash123";
            var fullName = "John Doe";

            // Act
            var user = new User(email, passwordHash, fullName);

            // Assert
            user.Email.Should().Be(email);
            user.PasswordHash.Should().Be(passwordHash);
            user.FullName.Should().Be(fullName);
            user.Role.Should().Be(UserRole.User);
            user.IsActive.Should().BeTrue();
            user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [TestCase("", "hash", "name", "*Email*")]
        [TestCase("email", "", "name", "*Password*")]
        [TestCase("email", "hash", "", "*Nombre*")]
        public void Constructor_WithInvalidArguments_ShouldThrowArgumentException(string email, string hash, string name, string expectedMessage)
        {
            // Act
            Action act = () => new User(email, hash, name);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage(expectedMessage);
        }

        [Test]
        public void ActivateDeactivate_ShouldChangeIsActiveStatus()
        {
            // Arrange
            var user = new User("test@test.com", "hash", "name");
            user.IsActive.Should().BeTrue();

            // Act - Deactivate
            user.Deactivate();
            user.IsActive.Should().BeFalse();

            // Act - Activate
            user.Activate();
            user.IsActive.Should().BeTrue();
        }

        [Test]
        public void UpdateRole_ShouldChangeUserRole()
        {
            // Arrange
            var user = new User("test@test.com", "hash", "name");

            // Act
            user.UpdateRole(UserRole.Admin);

            // Assert
            user.Role.Should().Be(UserRole.Admin);
        }

        [Test]
        public void SetResetPasswordToken_ShouldUpdateTokenAndExpiration()
        {
            // Arrange
            var user = new User("test@test.com", "hash", "name");
            var token = "token123";
            var expiration = DateTime.UtcNow.AddHours(1);

            // Act
            user.SetResetPasswordToken(token, expiration);

            // Assert
            user.ResetPasswordToken.Should().Be(token);
            user.ResetPasswordTokenExpiresAt.Should().Be(expiration);
        }
    }
}
