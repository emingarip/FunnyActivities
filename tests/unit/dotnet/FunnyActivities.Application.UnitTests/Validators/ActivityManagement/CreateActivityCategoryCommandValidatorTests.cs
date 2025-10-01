using System;
using FluentAssertions;
using FluentValidation.TestHelper;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.Validators.ActivityManagement;
using Xunit;

namespace FunnyActivities.Application.UnitTests.Validators.ActivityManagement
{
    public class CreateActivityCategoryCommandValidatorTests
    {
        private readonly CreateActivityCategoryCommandValidator _validator;

        public CreateActivityCategoryCommandValidatorTests()
        {
            _validator = new CreateActivityCategoryCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            // Arrange
            var command = new CreateActivityCategoryCommand
            {
                Name = "",
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Category name is required.");
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Null()
        {
            // Arrange
            var command = new CreateActivityCategoryCommand
            {
                Name = null,
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Category name is required.");
        }

        [Fact]
        public void Should_Have_Error_When_Name_Exceeds_MaxLength()
        {
            // Arrange
            var command = new CreateActivityCategoryCommand
            {
                Name = new string('A', 101), // 101 characters
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Category name must be between 1 and 100 characters.");
        }

        [Fact]
        public void Should_Have_Error_When_Description_Exceeds_MaxLength()
        {
            // Arrange
            var command = new CreateActivityCategoryCommand
            {
                Name = "Valid Name",
                Description = new string('A', 501), // 501 characters
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                .WithErrorMessage("Category description cannot exceed 500 characters.");
        }

        [Fact]
        public void Should_Have_Error_When_UserId_Is_Empty()
        {
            // Arrange
            var command = new CreateActivityCategoryCommand
            {
                Name = "Valid Name",
                UserId = Guid.Empty
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                .WithErrorMessage("User ID is required.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Command_Is_Valid()
        {
            // Arrange
            var command = new CreateActivityCategoryCommand
            {
                Name = "Valid Name",
                Description = "Valid Description",
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Not_Have_Error_When_Description_Is_Null()
        {
            // Arrange
            var command = new CreateActivityCategoryCommand
            {
                Name = "Valid Name",
                Description = null,
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}