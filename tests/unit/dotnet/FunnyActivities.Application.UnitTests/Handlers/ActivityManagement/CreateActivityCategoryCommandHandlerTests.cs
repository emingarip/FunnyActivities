using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.DTOs.ActivityManagement;
using FunnyActivities.Application.Handlers.ActivityManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FunnyActivities.Application.UnitTests.Handlers.ActivityManagement
{
    public class CreateActivityCategoryCommandHandlerTests
    {
        private readonly Mock<IActivityCategoryRepository> _repositoryMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<CreateActivityCategoryCommandHandler>> _loggerMock;
        private readonly CreateActivityCategoryCommandHandler _handler;

        public CreateActivityCategoryCommandHandlerTests()
        {
            _repositoryMock = new Mock<IActivityCategoryRepository>();
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<CreateActivityCategoryCommandHandler>>();
            _handler = new CreateActivityCategoryCommandHandler(
                _repositoryMock.Object,
                _mediatorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateActivityCategory_WhenNameIsUnique()
        {
            // Arrange
            var command = new CreateActivityCategoryCommand
            {
                Name = "New Category",
                Description = "Description",
                UserId = Guid.NewGuid()
            };

            _repositoryMock.Setup(r => r.ExistsByNameAsync(command.Name))
                .ReturnsAsync(false);
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<FunnyActivities.Domain.Entities.ActivityCategory>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ActivityCategoryDto>();
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);

            _repositoryMock.Verify(r => r.ExistsByNameAsync(command.Name), Times.Once);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<FunnyActivities.Domain.Entities.ActivityCategory>()), Times.Once);
            _mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowActivityCategoryNameAlreadyExistsException_WhenNameExists()
        {
            // Arrange
            var command = new CreateActivityCategoryCommand
            {
                Name = "Existing Category",
                Description = "Description",
                UserId = Guid.NewGuid()
            };

            _repositoryMock.Setup(r => r.ExistsByNameAsync(command.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<ActivityCategoryNameAlreadyExistsException>(() =>
                _handler.Handle(command, CancellationToken.None));

            _repositoryMock.Verify(r => r.ExistsByNameAsync(command.Name), Times.Once);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<FunnyActivities.Domain.Entities.ActivityCategory>()), Times.Never);
            _mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowOperationCanceledException_WhenCancellationRequested()
        {
            // Arrange
            var command = new CreateActivityCategoryCommand
            {
                Name = "New Category",
                Description = "Description",
                UserId = Guid.NewGuid()
            };
            var cancellationToken = new CancellationToken(true);

            _repositoryMock.Setup(r => r.ExistsByNameAsync(command.Name))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _handler.Handle(command, cancellationToken));
        }
    }
}