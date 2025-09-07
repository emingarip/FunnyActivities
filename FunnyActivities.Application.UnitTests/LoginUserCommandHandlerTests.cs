using FluentAssertions;
using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Application.DTOs.UserManagement;
using FunnyActivities.Application.Handlers.UserManagement;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Services;
using Moq;
using System.Security.Claims;
using Xunit;

namespace FunnyActivities.Application.UnitTests
{
    public class LoginUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<FunnyActivities.Domain.Interfaces.IJwtTokenService> _jwtTokenServiceMock;
        private readonly UserService _userService;
        private readonly LoginUserCommandHandler _handler;

        public LoginUserCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtTokenServiceMock = new Mock<FunnyActivities.Domain.Interfaces.IJwtTokenService>();
            _userService = new UserService();

            _handler = new LoginUserCommandHandler(
                _userRepositoryMock.Object,
                _userService,
                _jwtTokenServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnLoginResponse_WhenCredentialsAreValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User(userId, "test@example.com", "hashedpassword", "John", "Doe");
            var command = new LoginUserCommand { Email = "test@example.com", Password = "hashedpassword" };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync(user);
            _jwtTokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<IEnumerable<Claim>>()))
                .Returns("jwt-token");
            _jwtTokenServiceMock.Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be("jwt-token");
            result.RefreshToken.Should().Be("refresh-token");
            result.User.Should().NotBeNull();
            result.User.Id.Should().Be(userId);
            result.User.Email.Should().Be("test@example.com");
            result.User.FirstName.Should().Be("John");
            result.User.LastName.Should().Be("Doe");
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserNotFound()
        {
            // Arrange
            var command = new LoginUserCommand { Email = "nonexistent@example.com", Password = "password" };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenPasswordIsInvalid()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "test@example.com", "hashedpassword", "John", "Doe");
            var command = new LoginUserCommand { Email = "test@example.com", Password = "wrongpassword" };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldGenerateTokenWithCorrectClaims()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User(userId, "test@example.com", "hashedpassword", "John", "Doe");
            var command = new LoginUserCommand { Email = "test@example.com", Password = "hashedpassword" };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email))
                .ReturnsAsync(user);
            _jwtTokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<IEnumerable<Claim>>()))
                .Returns("jwt-token")
                .Callback<IEnumerable<Claim>>(claims =>
                {
                    var claimsList = claims.ToList();
                    claimsList.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
                    claimsList.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
                    claimsList.Should().Contain(c => c.Type == ClaimTypes.GivenName && c.Value == "John");
                    claimsList.Should().Contain(c => c.Type == ClaimTypes.Surname && c.Value == "Doe");
                });
            _jwtTokenServiceMock.Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _jwtTokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<IEnumerable<Claim>>()), Times.Once);
            _jwtTokenServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
        }
    }
}