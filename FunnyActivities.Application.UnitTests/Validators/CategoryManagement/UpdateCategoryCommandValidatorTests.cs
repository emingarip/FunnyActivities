using System;
using FluentValidation.TestHelper;
using Xunit;
using FunnyActivities.Application.Commands.CategoryManagement;
using FunnyActivities.Application.Validators.CategoryManagement;

namespace FunnyActivities.Application.UnitTests.Validators.CategoryManagement
{
    public class UpdateCategoryCommandValidatorTests
    {
        private readonly UpdateCategoryCommandValidator _validator;

        public UpdateCategoryCommandValidatorTests()
        {
            _validator = new UpdateCategoryCommandValidator();
        }

        [Fact]
        public void Validate_ValidCommand_ShouldPass()
        {
            // Arrange
            var command = new UpdateCategoryCommand
            {
                Id = Guid.NewGuid(),
                Name = "Valid Category Name",
                Description = "Valid description for the category",
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_ValidCommandWithoutDescription_ShouldPass()
        {
            // Arrange
            var command = new UpdateCategoryCommand
            {
                Id = Guid.NewGuid(),
                Name = "Valid Category Name",
                Description = null,
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_EmptyId_ShouldFail()
        {
            // Arrange
            var command = new UpdateCategoryCommand
            {
                Id = Guid.Empty,
                Name = "Valid Name",
                Description = "Valid description",
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Category ID is required.");
        }

        [Fact]
        public void Validate_EmptyName_ShouldFail()
        {
            // Arrange
            var command = new UpdateCategoryCommand
            {
                Id = Guid.NewGuid(),
                Name = "",
                Description = "Valid description",
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Category name is required.");
        }

        [Fact]
        public void Validate_NameTooLong_ShouldFail()
        {
            // Arrange
            var command = new UpdateCategoryCommand
            {
                Id = Guid.NewGuid(),
                Name = new string('A', 101),
                Description = "Valid description",
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Category name must be between 1 and 100 characters.");
        }

        [Fact]
        public void Validate_DescriptionTooLong_ShouldFail()
        {
            // Arrange
            var command = new UpdateCategoryCommand
            {
                Id = Guid.NewGuid(),
                Name = "Valid Name",
                Description = new string('A', 501),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Category description cannot exceed 500 characters.");
        }

        [Fact]
        public void Validate_EmptyUserId_ShouldFail()
        {
            // Arrange
            var command = new UpdateCategoryCommand
            {
                Id = Guid.NewGuid(),
                Name = "Valid Name",
                Description = "Valid description",
                UserId = Guid.Empty
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("User ID is required.");
        }
    }
}