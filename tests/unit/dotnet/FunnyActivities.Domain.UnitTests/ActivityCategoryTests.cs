using System;
using FluentAssertions;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Events;
using Xunit;

namespace FunnyActivities.Domain.UnitTests
{
    public class ActivityCategoryTests
    {
        [Fact]
        public void Create_ShouldCreateActivityCategoryWithCorrectProperties()
        {
            // Arrange
            var name = "Test Category";
            var description = "Test Description";

            // Act
            var category = ActivityCategory.Create(name, description);

            // Assert
            category.Name.Should().Be(name);
            category.Description.Should().Be(description);
            category.Activities.Should().NotBeNull().And.BeEmpty();
            category.DomainEvents.Should().ContainSingle(e => e is ActivityCategoryCreatedEvent);
        }

        [Fact]
        public void Create_ShouldThrowArgumentException_WhenNameIsNullOrEmpty()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => ActivityCategory.Create(null, "Description"));
            Assert.Throws<ArgumentException>(() => ActivityCategory.Create("", "Description"));
        }

        [Fact]
        public void UpdateDetails_ShouldUpdatePropertiesAndAddDomainEvent()
        {
            // Arrange
            var category = ActivityCategory.Create("Original Name", "Original Description");
            category.ClearDomainEvents();
            var newName = "Updated Name";
            var newDescription = "Updated Description";

            // Act
            category.UpdateDetails(newName, newDescription);

            // Assert
            category.Name.Should().Be(newName);
            category.Description.Should().Be(newDescription);
            category.DomainEvents.Should().ContainSingle(e => e is ActivityCategoryUpdatedEvent);
        }

        [Fact]
        public void ClearDomainEvents_ShouldClearAllDomainEvents()
        {
            // Arrange
            var category = ActivityCategory.Create("Test Category", null);
            category.DomainEvents.Should().NotBeEmpty();

            // Act
            category.ClearDomainEvents();

            // Assert
            category.DomainEvents.Should().BeEmpty();
        }
    }
}