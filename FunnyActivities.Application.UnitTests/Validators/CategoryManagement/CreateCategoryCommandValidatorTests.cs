using System;
using FluentValidation.TestHelper;
using Xunit;
using FunnyActivities.Application.Commands.CategoryManagement;
using FunnyActivities.Application.Validators.CategoryManagement;

namespace FunnyActivities.Application.UnitTests.Validators.CategoryManagement
{
    public class CreateCategoryCommandValidatorTests
    {
        private readonly CreateCategoryCommandValidator _validator;

        public CreateCategoryCommandValidatorTests()
        {
            _validator = new CreateCategoryCommandValidator();
        }

        [Fact]
        public void Validate_ValidCommand_ShouldPass()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
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
            var command = new CreateCategoryCommand
            {
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
        public void Validate_EmptyName_ShouldFail()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
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
        public void Validate_NullName_ShouldFail()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = null,
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
            var command = new CreateCategoryCommand
            {
                Name = new string('A', 101), // 101 characters
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
        public void Validate_NameAtMaxLength_ShouldPass()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = new string('A', 100), // Exactly 100 characters
                Description = "Valid description",
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Validate_DescriptionTooLong_ShouldFail()
        {
            // Arrange
            var command = new CreateCategoryCommand
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
        public void Validate_DescriptionAtMaxLength_ShouldPass()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "Valid Name",
                Description = new string('A', 500), // Exactly 500 characters
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Validate_EmptyUserId_ShouldFail()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
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

        [Fact]
        public void Validate_MultipleValidationErrors_ShouldFail()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "",
                Description = new string('A', 501),
                UserId = Guid.Empty
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
            result.ShouldHaveValidationErrorFor(x => x.Description);
            result.ShouldHaveValidationErrorFor(x => x.UserId);
            Assert.Equal(3, result.Errors.Count);
        }
    }
}