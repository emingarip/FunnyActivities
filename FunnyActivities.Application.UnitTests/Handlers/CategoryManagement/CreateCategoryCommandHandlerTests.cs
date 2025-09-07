using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Commands.CategoryManagement;
using FunnyActivities.Application.Handlers.CategoryManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.DTOs.CategoryManagement;
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.UnitTests.Handlers.CategoryManagement
{
    public class CreateCategoryCommandHandlerTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<CreateCategoryCommandHandler>> _loggerMock;
        private readonly CreateCategoryCommandHandler _handler;
        private readonly Fixture _fixture;

        public CreateCategoryCommandHandlerTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<CreateCategoryCommandHandler>>();
            _fixture = new Fixture();

            _handler = new CreateCategoryCommandHandler(
                _categoryRepositoryMock.Object,
                _mediatorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldCreateCategory()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "Test Category",
                Description = "Test Description",
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.ExistsByNameAsync(command.Name)).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.ProductCount.Should().Be(0);

            _categoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Category>()), Times.Once);
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommandWithoutDescription_ShouldCreateCategory()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "Test Category",
                Description = null,
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.ExistsByNameAsync(command.Name)).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(command.Name);
            result.Description.Should().BeNull();
            result.ProductCount.Should().Be(0);

            _categoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Category>()), Times.Once);
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateName_ShouldThrowException()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "Existing Category",
                Description = "Test Description",
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.ExistsByNameAsync(command.Name)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<CategoryNameAlreadyExistsException>(() =>
                _handler.Handle(command, CancellationToken.None));

            _categoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Category>()), Times.Never);
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EmptyName_ShouldThrowException()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "",
                Description = "Test Description",
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.ExistsByNameAsync(command.Name)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<CategoryNameAlreadyExistsException>(() =>
                _handler.Handle(command, CancellationToken.None));

            _categoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NullName_ShouldThrowException()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = null,
                Description = "Test Description",
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.ExistsByNameAsync(command.Name)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<CategoryNameAlreadyExistsException>(() =>
                _handler.Handle(command, CancellationToken.None));

            _categoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task Handle_RepositoryAddFails_ShouldNotPublishEvent()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "Test Category",
                Description = "Test Description",
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.ExistsByNameAsync(command.Name)).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Category>())).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _handler.Handle(command, CancellationToken.None));

            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CancellationRequested_ShouldCancelOperation()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "Test Category",
                Description = "Test Description",
                UserId = Guid.NewGuid()
            };

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            _categoryRepositoryMock.Setup(x => x.ExistsByNameAsync(command.Name)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                _handler.Handle(command, cancellationTokenSource.Token));
        }
    }
}