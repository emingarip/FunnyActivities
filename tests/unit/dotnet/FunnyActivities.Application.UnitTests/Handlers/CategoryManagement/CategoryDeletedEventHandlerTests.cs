using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Events;
using FunnyActivities.Application.Handlers.CategoryManagement;

namespace FunnyActivities.Application.UnitTests.Handlers.CategoryManagement
{
    public class CategoryDeletedEventHandlerTests
    {
        private readonly Mock<ILogger<CategoryDeletedEventHandler>> _loggerMock;
        private readonly CategoryDeletedEventHandler _handler;

        public CategoryDeletedEventHandlerTests()
        {
            _loggerMock = new Mock<ILogger<CategoryDeletedEventHandler>>();
            _handler = new CategoryDeletedEventHandler(_loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidEvent_ShouldLogInformation()
        {
            // Arrange
            var category = Category.Create("Deleted Category", "Deleted Description");
            typeof(Category).GetProperty("Id")?.SetValue(category, Guid.NewGuid());

            var categoryEvent = new CategoryDeletedEvent(category);

            // Act
            await _handler.Handle(categoryEvent, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Category deleted") &&
                                                   o.ToString().Contains(category.Id.ToString()) &&
                                                   o.ToString().Contains(category.Name)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CancellationRequested_ShouldCancelOperation()
        {
            // Arrange
            var category = Category.Create("Deleted Category", "Deleted Description");
            typeof(Category).GetProperty("Id")?.SetValue(category, Guid.NewGuid());

            var categoryEvent = new CategoryDeletedEvent(category);
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                _handler.Handle(categoryEvent, cancellationTokenSource.Token));
        }
    }
}