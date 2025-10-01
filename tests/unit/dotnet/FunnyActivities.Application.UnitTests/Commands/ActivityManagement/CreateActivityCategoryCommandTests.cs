using System;
using FluentAssertions;
using FunnyActivities.Application.Commands.ActivityManagement;
using Xunit;

namespace FunnyActivities.Application.UnitTests.Commands.ActivityManagement
{
    public class CreateActivityCategoryCommandTests
    {
        [Fact]
        public void CreateActivityCategoryCommand_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var name = "Test Category";
            var description = "Test Description";
            var userId = Guid.NewGuid();

            // Act
            var command = new CreateActivityCategoryCommand
            {
                Name = name,
                Description = description,
                UserId = userId
            };

            // Assert
            command.Name.Should().Be(name);
            command.Description.Should().Be(description);
            command.UserId.Should().Be(userId);
        }

        [Fact]
        public void CreateActivityCategoryCommand_ShouldAllowNullDescription()
        {
            // Arrange
            var name = "Test Category";
            var userId = Guid.NewGuid();

            // Act
            var command = new CreateActivityCategoryCommand
            {
                Name = name,
                Description = null,
                UserId = userId
            };

            // Assert
            command.Name.Should().Be(name);
            command.Description.Should().BeNull();
            command.UserId.Should().Be(userId);
        }
    }
}