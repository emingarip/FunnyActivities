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
using FunnyActivities.Domain.Exceptions;

namespace FunnyActivities.Application.UnitTests.Handlers.CategoryManagement
{
    public class DeleteCategoryCommandHandlerTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<DeleteCategoryCommandHandler>> _loggerMock;
        private readonly DeleteCategoryCommandHandler _handler;
        private readonly Fixture _fixture;

        public DeleteCategoryCommandHandlerTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<DeleteCategoryCommandHandler>>();
            _fixture = new Fixture();

            _handler = new DeleteCategoryCommandHandler(
                _categoryRepositoryMock.Object,
                _mediatorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldDeleteCategory()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var existingCategory = Category.Create("Test Category", "Test Description");
            typeof(Category).GetProperty("Id")?.SetValue(existingCategory, categoryId);

            var command = new DeleteCategoryCommand
            {
                Id = categoryId,
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(existingCategory);
            _categoryRepositoryMock.Setup(x => x.DeleteAsync(existingCategory)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);

            _categoryRepositoryMock.Verify(x => x.DeleteAsync(existingCategory), Times.Once);
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CategoryNotFound_ShouldThrowException()
        {
            // Arrange
            var command = new DeleteCategoryCommand
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(command.Id)).ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<CategoryNotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            _categoryRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Category>()), Times.Never);
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CategoryWithProducts_ShouldStillDelete()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var existingCategory = Category.Create("Test Category", "Test Description");
            typeof(Category).GetProperty("Id")?.SetValue(existingCategory, categoryId);

            // Simulate category with products
            var products = new System.Collections.Generic.List<BaseProduct>
            {
                new BaseProduct(Guid.NewGuid(), "Product 1", "Description", categoryId),
                new BaseProduct(Guid.NewGuid(), "Product 2", "Description", categoryId)
            };
            typeof(Category).GetProperty("BaseProducts")?.SetValue(existingCategory, products);

            var command = new DeleteCategoryCommand
            {
                Id = categoryId,
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(existingCategory);
            _categoryRepositoryMock.Setup(x => x.DeleteAsync(existingCategory)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);

            _categoryRepositoryMock.Verify(x => x.DeleteAsync(existingCategory), Times.Once);
            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_RepositoryDeleteFails_ShouldNotPublishEvent()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var existingCategory = Category.Create("Test Category", "Test Description");
            typeof(Category).GetProperty("Id")?.SetValue(existingCategory, categoryId);

            var command = new DeleteCategoryCommand
            {
                Id = categoryId,
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(existingCategory);
            _categoryRepositoryMock.Setup(x => x.DeleteAsync(existingCategory)).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _handler.Handle(command, CancellationToken.None));

            _mediatorMock.Verify(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CancellationRequested_ShouldCancelOperation()
        {
            // Arrange
            var command = new DeleteCategoryCommand
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                _handler.Handle(command, cancellationTokenSource.Token));
        }

        [Fact]
        public async Task Handle_EmptyGuid_ShouldThrowException()
        {
            // Arrange
            var command = new DeleteCategoryCommand
            {
                Id = Guid.Empty,
                UserId = Guid.NewGuid()
            };

            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(command.Id)).ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<CategoryNotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            _categoryRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Category>()), Times.Never);
        }
    }
}