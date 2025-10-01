using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.ValueObjects;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.Handlers.ActivityManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.DTOs.ActivityManagement;

namespace FunnyActivities.Application.UnitTests.Handlers.ActivityManagement
{
    public class UpdateActivityCommandHandlerTests
    {
        private readonly Mock<IActivityRepository> _activityRepositoryMock;
        private readonly Mock<ILogger<UpdateActivityCommandHandler>> _loggerMock;
        private readonly UpdateActivityCommandHandler _handler;
        private readonly Fixture _fixture;

        public UpdateActivityCommandHandlerTests()
        {
            _activityRepositoryMock = new Mock<IActivityRepository>();
            _loggerMock = new Mock<ILogger<UpdateActivityCommandHandler>>();
            _fixture = new Fixture();

            _handler = new UpdateActivityCommandHandler(
                _activityRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_UpdateDurationOnly_ShouldPreserveExistingVideo()
        {
            // Arrange
            var activityId = Guid.NewGuid();
            var existingVideoUrl = VideoUrl.Create("https://example.com/existing-video.mp4");
            var existingDuration = Duration.Create(1, 0, 0);
            var activity = Activity.Create("Test Activity", "Description", existingVideoUrl, existingDuration, Guid.NewGuid());

            var command = new UpdateActivityCommand
            {
                Id = activityId,
                Name = "Updated Activity",
                Description = "Updated Description",
                DurationHours = 2,
                DurationMinutes = 30,
                DurationSeconds = 15,
                // VideoUrl not provided - should preserve existing
                UserId = Guid.NewGuid()
            };

            _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId)).ReturnsAsync(activity);
            _activityRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Activity>())).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);
            result.VideoUrl.Should().Be(existingVideoUrl.Value); // Should preserve existing video
            result.Duration.Should().Be("02:30:15"); // Should update duration

            _activityRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Activity>(a =>
                a.VideoUrl == existingVideoUrl && // Video preserved
                a.Duration.ToString() == "02:30:15" // Duration updated
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateVideoOnly_ShouldPreserveExistingDuration()
        {
            // Arrange
            var activityId = Guid.NewGuid();
            var existingVideoUrl = VideoUrl.Create("https://example.com/old-video.mp4");
            var existingDuration = Duration.Create(1, 30, 45);
            var activity = Activity.Create("Test Activity", "Description", existingVideoUrl, existingDuration, Guid.NewGuid());

            var newVideoUrl = "https://example.com/new-video.mp4";
            var command = new UpdateActivityCommand
            {
                Id = activityId,
                Name = "Updated Activity",
                Description = "Updated Description",
                VideoUrl = newVideoUrl,
                // Duration fields not provided - should preserve existing
                UserId = Guid.NewGuid()
            };

            _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId)).ReturnsAsync(activity);
            _activityRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Activity>())).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);
            result.VideoUrl.Should().Be(newVideoUrl); // Should update video
            result.Duration.Should().Be(existingDuration.ToString()); // Should preserve existing duration

            _activityRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Activity>(a =>
                a.VideoUrl.Value == newVideoUrl && // Video updated
                a.Duration == existingDuration // Duration preserved
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_FullUpdate_ShouldUpdateBothVideoAndDuration()
        {
            // Arrange
            var activityId = Guid.NewGuid();
            var existingVideoUrl = VideoUrl.Create("https://example.com/old-video.mp4");
            var existingDuration = Duration.Create(1, 0, 0);
            var activity = Activity.Create("Test Activity", "Description", existingVideoUrl, existingDuration, Guid.NewGuid());

            var newVideoUrl = "https://example.com/new-video.mp4";
            var command = new UpdateActivityCommand
            {
                Id = activityId,
                Name = "Updated Activity",
                Description = "Updated Description",
                VideoUrl = newVideoUrl,
                DurationHours = 3,
                DurationMinutes = 45,
                DurationSeconds = 30,
                UserId = Guid.NewGuid()
            };

            _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId)).ReturnsAsync(activity);
            _activityRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Activity>())).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);
            result.VideoUrl.Should().Be(newVideoUrl);
            result.Duration.Should().Be("03:45:30");

            _activityRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Activity>(a =>
                a.VideoUrl.Value == newVideoUrl &&
                a.Duration.ToString() == "03:45:30"
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateWithZeroDuration_ShouldSetDurationToZero()
        {
            // Arrange
            var activityId = Guid.NewGuid();
            var existingVideoUrl = VideoUrl.Create("https://example.com/video.mp4");
            var existingDuration = Duration.Create(1, 30, 45);
            var activity = Activity.Create("Test Activity", "Description", existingVideoUrl, existingDuration, Guid.NewGuid());

            var command = new UpdateActivityCommand
            {
                Id = activityId,
                Name = "Updated Activity",
                Description = "Updated Description",
                DurationHours = 0,
                DurationMinutes = 0,
                DurationSeconds = 0,
                // VideoUrl not provided - should preserve existing
                UserId = Guid.NewGuid()
            };

            _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId)).ReturnsAsync(activity);
            _activityRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Activity>())).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.VideoUrl.Should().Be(existingVideoUrl.Value); // Preserved
            result.Duration.Should().Be("00:00:00"); // Set to zero

            _activityRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Activity>(a =>
                a.VideoUrl == existingVideoUrl &&
                a.Duration.ToString() == "00:00:00"
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_ActivityNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var command = new UpdateActivityCommand
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                UserId = Guid.NewGuid()
            };

            _activityRepositoryMock.Setup(x => x.GetByIdAsync(command.Id)).ReturnsAsync((Activity)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldReturnActivityDto()
        {
            // Arrange
            var activityId = Guid.NewGuid();
            var activityCategoryId = Guid.NewGuid();
            var activity = Activity.Create("Test Activity", "Description", null, null, activityCategoryId);

            var command = new UpdateActivityCommand
            {
                Id = activityId,
                Name = "Updated Name",
                Description = "Updated Description",
                UserId = Guid.NewGuid()
            };

            _activityRepositoryMock.Setup(x => x.GetByIdAsync(activityId)).ReturnsAsync(activity);
            _activityRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Activity>())).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ActivityDto>();
            result.Id.Should().Be(activityId);
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);
        }
    }
}