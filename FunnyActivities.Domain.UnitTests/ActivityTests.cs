using System;
using FluentAssertions;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Events;
using FunnyActivities.Domain.ValueObjects;
using Xunit;

namespace FunnyActivities.Domain.UnitTests
{
    public class ActivityTests
    {
        [Fact]
        public void Create_ShouldCreateActivityWithCorrectProperties()
        {
            // Arrange
            var name = "Test Activity";
            var description = "Test Description";
            var videoUrl = VideoUrl.Create("https://example.com/video.mp4");
            var duration = Duration.Create(1, 30, 0);
            var activityCategoryId = Guid.NewGuid();

            // Act
            var activity = Activity.Create(name, description, videoUrl, duration, activityCategoryId);

            // Assert
            activity.Name.Should().Be(name);
            activity.Description.Should().Be(description);
            activity.VideoUrl.Should().Be(videoUrl);
            activity.Duration.Should().Be(duration);
            activity.ActivityCategoryId.Should().Be(activityCategoryId);
            activity.Steps.Should().NotBeNull().And.BeEmpty();
            activity.ActivityProductVariants.Should().NotBeNull().And.BeEmpty();
            activity.DomainEvents.Should().ContainSingle(e => e is ActivityCreatedEvent);
        }

        [Fact]
        public void Create_ShouldThrowArgumentException_WhenNameIsNullOrEmpty()
        {
            // Arrange
            var activityCategoryId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => Activity.Create(null, "Description", null, null, activityCategoryId));
            Assert.Throws<ArgumentException>(() => Activity.Create("", "Description", null, null, activityCategoryId));
        }

        [Fact]
        public void UpdateDetails_ShouldUpdatePropertiesAndAddDomainEvent()
        {
            // Arrange
            var activity = Activity.Create("Original Name", "Original Description", null, null, Guid.NewGuid());
            activity.ClearDomainEvents();
            var newName = "Updated Name";
            var newDescription = "Updated Description";
            var newVideoUrl = VideoUrl.Create("https://example.com/new-video.mp4");
            var newDuration = Duration.Create(2, 0, 0);

            // Act
            activity.UpdateDetails(newName, newDescription, newVideoUrl, newDuration);

            // Assert
            activity.Name.Should().Be(newName);
            activity.Description.Should().Be(newDescription);
            activity.VideoUrl.Should().Be(newVideoUrl);
            activity.Duration.Should().Be(newDuration);
            activity.DomainEvents.Should().ContainSingle(e => e is ActivityUpdatedEvent);
        }

        [Fact]
        public void AddStep_ShouldAddStepToActivity()
        {
            // Arrange
            var activity = Activity.Create("Test Activity", null, null, null, Guid.NewGuid());
            var step = Step.Create(activity.Id, 1, "Test Step");

            // Act
            activity.AddStep(step);

            // Assert
            activity.Steps.Should().Contain(step);
        }

        [Fact]
        public void RemoveStep_ShouldRemoveStepFromActivity()
        {
            // Arrange
            var activity = Activity.Create("Test Activity", null, null, null, Guid.NewGuid());
            var step = Step.Create(activity.Id, 1, "Test Step");
            activity.AddStep(step);

            // Act
            activity.RemoveStep(step);

            // Assert
            activity.Steps.Should().NotContain(step);
        }

        [Fact]
        public void AddProductVariant_ShouldAddProductVariantToActivity()
        {
            // Arrange
            var activity = Activity.Create("Test Activity", null, null, null, Guid.NewGuid());
            var productVariantId = Guid.NewGuid();
            var unitOfMeasureId = Guid.NewGuid();
            var activityProductVariant = ActivityProductVariant.Create(activity.Id, productVariantId, 1.5m, unitOfMeasureId);

            // Act
            activity.AddProductVariant(activityProductVariant);

            // Assert
            activity.ActivityProductVariants.Should().Contain(activityProductVariant);
        }

        [Fact]
        public void RemoveProductVariant_ShouldRemoveProductVariantFromActivity()
        {
            // Arrange
            var activity = Activity.Create("Test Activity", null, null, null, Guid.NewGuid());
            var productVariantId = Guid.NewGuid();
            var unitOfMeasureId = Guid.NewGuid();
            var activityProductVariant = ActivityProductVariant.Create(activity.Id, productVariantId, 1.5m, unitOfMeasureId);
            activity.AddProductVariant(activityProductVariant);

            // Act
            activity.RemoveProductVariant(activityProductVariant);

            // Assert
            activity.ActivityProductVariants.Should().NotContain(activityProductVariant);
        }

        [Fact]
        public void ClearDomainEvents_ShouldClearAllDomainEvents()
        {
            // Arrange
            var activity = Activity.Create("Test Activity", null, null, null, Guid.NewGuid());
            activity.DomainEvents.Should().NotBeEmpty();

            // Act
            activity.ClearDomainEvents();

            // Assert
            activity.DomainEvents.Should().BeEmpty();
        }
    }
}