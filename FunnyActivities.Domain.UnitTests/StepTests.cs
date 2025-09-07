using System;
using FluentAssertions;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Events;
using Xunit;

namespace FunnyActivities.Domain.UnitTests
{
    public class StepTests
    {
        [Fact]
        public void Create_ShouldCreateStepWithCorrectProperties()
        {
            // Arrange
            var activityId = Guid.NewGuid();
            var order = 1;
            var description = "Test Step Description";

            // Act
            var step = Step.Create(activityId, order, description);

            // Assert
            step.ActivityId.Should().Be(activityId);
            step.Order.Should().Be(order);
            step.Description.Should().Be(description);
            step.DomainEvents.Should().ContainSingle(e => e is StepCreatedEvent);
        }

        [Fact]
        public void Create_ShouldThrowArgumentException_WhenDescriptionIsNullOrEmpty()
        {
            // Arrange
            var activityId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => Step.Create(activityId, 1, null));
            Assert.Throws<ArgumentException>(() => Step.Create(activityId, 1, ""));
        }

        [Fact]
        public void UpdateDetails_ShouldUpdatePropertiesAndAddDomainEvent()
        {
            // Arrange
            var step = Step.Create(Guid.NewGuid(), 1, "Original Description");
            step.ClearDomainEvents();
            var newOrder = 2;
            var newDescription = "Updated Description";

            // Act
            step.UpdateDetails(newOrder, newDescription);

            // Assert
            step.Order.Should().Be(newOrder);
            step.Description.Should().Be(newDescription);
            step.DomainEvents.Should().ContainSingle(e => e is StepUpdatedEvent);
        }

        [Fact]
        public void ClearDomainEvents_ShouldClearAllDomainEvents()
        {
            // Arrange
            var step = Step.Create(Guid.NewGuid(), 1, "Test Step");
            step.DomainEvents.Should().NotBeEmpty();

            // Act
            step.ClearDomainEvents();

            // Assert
            step.DomainEvents.Should().BeEmpty();
        }
    }
}