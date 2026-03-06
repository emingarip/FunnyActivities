using System.Security.Claims;
using FluentAssertions;
using FunnyActivities.Application.Services;
using FunnyActivities.CrossCuttingConcerns.Authentication;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Domain.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace FunnyActivities.Application.UnitTests.Services;

public class GoogleLoginServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IGoogleTokenVerifier> _googleTokenVerifierMock;
    private readonly Mock<ILogger<GoogleLoginService>> _loggerMock;
    private readonly GoogleLoginService _service;

    public GoogleLoginServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _googleTokenVerifierMock = new Mock<IGoogleTokenVerifier>();
        _loggerMock = new Mock<ILogger<GoogleLoginService>>();

        _service = new GoogleLoginService(
            _userRepositoryMock.Object,
            new UserService(),
            _jwtTokenServiceMock.Object,
            _googleTokenVerifierMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldCreateUser_WhenEmailDoesNotExist()
    {
        const string idToken = "google-id-token";
        User? createdUser = null;

        _googleTokenVerifierMock
            .Setup(x => x.VerifyAsync(idToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo
            {
                Email = " New.User@Example.com ",
                GivenName = "New",
                FamilyName = "User",
                Subject = "google-sub"
            });

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("new.user@example.com"))
            .ReturnsAsync((User?)null);
        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Callback<User>(user => createdUser = user)
            .Returns(Task.CompletedTask);
        _jwtTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<IEnumerable<Claim>>()))
            .Returns("jwt-token");
        _jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        var result = await _service.AuthenticateAsync(idToken);

        createdUser.Should().NotBeNull();
        createdUser!.Email.Should().Be("new.user@example.com");
        createdUser.Role.Should().Be(UserRole.User);
        createdUser.FirstName.Should().Be("New");
        createdUser.LastName.Should().Be("User");
        createdUser.LastLoginDate.Should().NotBeNull();
        result.Token.Should().Be("jwt-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.User.Email.Should().Be("new.user@example.com");
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReuseExistingUser_WhenEmailExists()
    {
        const string idToken = "google-id-token";
        var existingUser = new User(Guid.NewGuid(), "existing@example.com", "hash", "Existing", "User");

        _googleTokenVerifierMock
            .Setup(x => x.VerifyAsync(idToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo
            {
                Email = "existing@example.com",
                GivenName = "Ignored",
                FamilyName = "Ignored",
                Subject = "google-sub"
            });
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("existing@example.com"))
            .ReturnsAsync(existingUser);
        _jwtTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<IEnumerable<Claim>>()))
            .Returns("jwt-token");
        _jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        var result = await _service.AuthenticateAsync(idToken);

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.Verify(x => x.UpdateAsync(existingUser), Times.Once);
        result.User.Id.Should().Be(existingUser.Id);
        result.User.Email.Should().Be("existing@example.com");
        existingUser.LastLoginDate.Should().NotBeNull();
    }
}
