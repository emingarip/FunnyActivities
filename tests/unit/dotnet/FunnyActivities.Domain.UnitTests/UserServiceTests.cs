using FluentAssertions;
using FunnyActivities.Domain.Services;
using FunnyActivities.Domain.ValueObjects;
using Xunit;

namespace FunnyActivities.Domain.UnitTests
{
    public class UserServiceTests
    {
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userService = new UserService();
        }

        [Fact]
        public void IsValidEmail_ShouldReturnTrueForValidEmail()
        {
            // Arrange
            var email = new Email("test@example.com");

            // Act
            var result = _userService.IsValidEmail(email);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsValidEmail_ShouldReturnFalseForInvalidEmail()
        {
            // Arrange
            var email = new Email("invalidemail");

            // Act
            var result = _userService.IsValidEmail(email);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void HashPassword_ShouldReturnHashedPassword()
        {
            // Arrange
            var password = new Password("testpassword123");

            // Act
            var hashedPassword = _userService.HashPassword(password);

            // Assert
            hashedPassword.Should().NotBeNullOrEmpty();
            hashedPassword.Should().NotBe(password.Value);
        }

        [Fact]
        public void HashPassword_ShouldReturnConsistentHashForSamePassword()
        {
            // Arrange
            var password1 = new Password("testpassword123");
            var password2 = new Password("testpassword123");

            // Act
            var hash1 = _userService.HashPassword(password1);
            var hash2 = _userService.HashPassword(password2);

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
        {
            // Arrange
            var password = new Password("testpassword123");
            var hashedPassword = _userService.HashPassword(password);

            // Act
            var result = _userService.VerifyPassword(hashedPassword, password.Value);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
        {
            // Arrange
            var password = new Password("testpassword123");
            var hashedPassword = _userService.HashPassword(password);
            var wrongPassword = "wrongpassword";

            // Act
            var result = _userService.VerifyPassword(hashedPassword, wrongPassword);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalseForNullHashedPassword()
        {
            // Arrange
            var plainPassword = "testpassword123";

            // Act
            var result = _userService.VerifyPassword(null, plainPassword);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalseForEmptyHashedPassword()
        {
            // Arrange
            var plainPassword = "testpassword123";

            // Act
            var result = _userService.VerifyPassword("", plainPassword);

            // Assert
            result.Should().BeFalse();
        }
    }
}