using FluentAssertions;
using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Application.Handlers;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Domain.Services;
using FunnyActivities.Domain.ValueObjects;
using Moq;
using Xunit;

namespace FunnyActivities.Application.UnitTests.Handlers.UserManagement
{
    public class ChangePasswordCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;
        private readonly ChangePasswordCommandHandler _handler;

        public ChangePasswordCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService();
            _handler = new ChangePasswordCommandHandler(_userRepositoryMock.Object, _userService);
        }

        [Fact]
        public async Task Handle_ShouldUpdatePassword_WhenCurrentPasswordIsValid()
        {
            var currentPassword = "Password123";
            var newPassword = "NewPassword456";
            var user = new User(Guid.NewGuid(), "user@example.com", _userService.HashPassword(new Password(currentPassword)), "Jane", "Doe");

            _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            await _handler.Handle(new ChangePasswordCommand
            {
                UserId = user.Id,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            }, CancellationToken.None);

            _userService.VerifyPassword(user.PasswordHash, newPassword).Should().BeTrue();
            _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenCurrentPasswordIsInvalid()
        {
            var user = new User(Guid.NewGuid(), "user@example.com", _userService.HashPassword(new Password("Password123")), "Jane", "Doe");

            _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);

            var action = () => _handler.Handle(new ChangePasswordCommand
            {
                UserId = user.Id,
                CurrentPassword = "WrongPassword999",
                NewPassword = "NewPassword456"
            }, CancellationToken.None);

            await action.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Current password is incorrect");
        }
    }
}
