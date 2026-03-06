using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FunnyActivities.Application.Commands.NotificationSystem;
using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Application.Handlers.UserManagement;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Domain.Services;
using MediatR;
using Moq;
using Xunit;

namespace FunnyActivities.Application.UnitTests.Handlers.UserManagement
{
    public class RegisterUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly RegisterUserCommandHandler _handler;

        public RegisterUserCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _mediatorMock = new Mock<IMediator>();

            _handler = new RegisterUserCommandHandler(
                _userRepositoryMock.Object,
                new UserService(),
                _mediatorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldAlwaysCreateRegularUser()
        {
            // Registration must never trust caller-supplied role data.
            var command = new RegisterUserCommand
            {
                Email = " New-User@Example.com ",
                Password = "Password123!",
                FirstName = "New",
                LastName = "User"
            };
            User? createdUser = null;
            SendRegistrationConfirmationEmailCommand? emailCommand = null;

            _userRepositoryMock.Setup(x => x.ExistsByEmailAsync("new-user@example.com"))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                .Callback<User>(user => createdUser = user)
                .Returns(Task.CompletedTask);
            _mediatorMock.Setup(x => x.Send(It.IsAny<SendRegistrationConfirmationEmailCommand>(), It.IsAny<CancellationToken>()))
                .Callback<SendRegistrationConfirmationEmailCommand, CancellationToken>((request, _) => emailCommand = request)
                .Returns(Task.CompletedTask);

            await _handler.Handle(command, CancellationToken.None);

            createdUser.Should().NotBeNull();
            createdUser!.Email.Should().Be("new-user@example.com");
            createdUser!.Role.Should().Be(UserRole.User);
            emailCommand.Should().NotBeNull();
            emailCommand!.Email.Should().Be("new-user@example.com");
        }
    }
}
