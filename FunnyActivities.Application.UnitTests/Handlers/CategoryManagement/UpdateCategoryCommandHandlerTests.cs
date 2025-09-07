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
    public class UpdateCategoryCommandHandlerTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<UpdateCategoryCommandHandler>> _loggerMock;
        private readonly UpdateCategoryCommandHandler _handler;
        private readonly Fixture _fixture;

        public UpdateCategoryCommandHandlerTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<UpdateCategoryCommandHandler>>();
            _fixture = new Fixture();

            _handler = new UpdateCategoryCommandHandler(
                _categoryRepositoryMock.Object,
                _mediatorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldUpdateCategory()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var existingCategory = Category.Create("Old Name", "Old Description");
            typeof(Category).GetProperty("Id")?.SetValue(existingCategory, categoryId);

            var command = new UpdateCategoryCommand
            {
                Id = categoryId,
                Name = "New Name",
                Description = "New Description",
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(existingCategory);
            _categoryRepositoryMock.Setup(x => x.ExistsByNameExcludingIdAsync(command.Name, categoryId)).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.UpdateAsync(existingCategory)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(categoryId);
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);
            result.UpdatedAt.Should().BeAfter(result.CreatedAt);

            _categoryRepositoryMock.Verify(x => x.UpdateAsync(existingCategory), Times.Once);
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateDescriptionOnly_ShouldUpdateCategory()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var existingCategory = Category.Create("Test Name", "Old Description");
            typeof(Category).GetProperty("Id")?.SetValue(existingCategory, categoryId);

            var command = new UpdateCategoryCommand
            {
                Id = categoryId,
                Name = "Test Name",
                Description = "New Description",
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(existingCategory);
            _categoryRepositoryMock.Setup(x => x.ExistsByNameExcludingIdAsync(command.Name, categoryId)).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.UpdateAsync(existingCategory)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);

            _categoryRepositoryMock.Verify(x => x.UpdateAsync(existingCategory), Times.Once);
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateToNullDescription_ShouldUpdateCategory()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var existingCategory = Category.Create("Test Name", "Old Description");
            typeof(Category).GetProperty("Id")?.SetValue(existingCategory, categoryId);

            var command = new UpdateCategoryCommand
            {
                Id = categoryId,
                Name = "Test Name",
                Description = null,
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(existingCategory);
            _categoryRepositoryMock.Setup(x => x.ExistsByNameExcludingIdAsync(command.Name, categoryId)).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.UpdateAsync(existingCategory)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().BeNull();

            _categoryRepositoryMock.Verify(x => x.UpdateAsync(existingCategory), Times.Once);
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CategoryNotFound_ShouldThrowException()
        {
            // Arrange
            var command = new UpdateCategoryCommand
            {
                Id = Guid.NewGuid(),
                Name = "New Name",
                Description = "New Description",
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(command.Id)).ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<CategoryNotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            _categoryRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>()), Times.Never);
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DuplicateName_ShouldThrowException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var existingCategory = Category.Create("Old Name", "Old Description");
            typeof(Category).GetProperty("Id")?.SetValue(existingCategory, categoryId);

            var command = new UpdateCategoryCommand
            {
                Id = categoryId,
                Name = "Existing Name",
                Description = "New Description",
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(existingCategory);
            _categoryRepositoryMock.Setup(x => x.ExistsByNameExcludingIdAsync(command.Name, categoryId)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<CategoryNameAlreadyExistsException>(() =>
                _handler.Handle(command, CancellationToken.None));

            _categoryRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>()), Times.Never);
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_SameNameAsCurrent_ShouldAllowUpdate()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var existingCategory = Category.Create("Same Name", "Old Description");
            typeof(Category).GetProperty("Id")?.SetValue(existingCategory, categoryId);

            var command = new UpdateCategoryCommand
            {
                Id = categoryId,
                Name = "Same Name",
                Description = "New Description",
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(existingCategory);
            _categoryRepositoryMock.Setup(x => x.ExistsByNameExcludingIdAsync(command.Name, categoryId)).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.UpdateAsync(existingCategory)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);

            _categoryRepositoryMock.Verify(x => x.UpdateAsync(existingCategory), Times.Once);
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_RepositoryUpdateFails_ShouldNotPublishEvent()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var existingCategory = Category.Create("Old Name", "Old Description");
            typeof(Category).GetProperty("Id")?.SetValue(existingCategory, categoryId);

            var command = new UpdateCategoryCommand
            {
                Id = categoryId,
                Name = "New Name",
                Description = "New Description",
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(existingCategory);
            _categoryRepositoryMock.Setup(x => x.ExistsByNameExcludingIdAsync(command.Name, categoryId)).ReturnsAsync(false);
            _categoryRepositoryMock.Setup(x => x.UpdateAsync(existingCategory)).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _handler.Handle(command, CancellationToken.None));

            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CancellationRequested_ShouldCancelOperation()
        {
            // Arrange
            var command = new UpdateCategoryCommand
            {
                Id = Guid.NewGuid(),
                Name = "New Name",
                Description = "New Description",
                UserId = Guid.NewGuid()
            };

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                _handler.Handle(command, cancellationTokenSource.Token));
        }
    }
}