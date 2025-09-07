using System;
using FluentValidation.TestHelper;
using Xunit;
using FunnyActivities.Application.Commands.CategoryManagement;
using FunnyActivities.Application.Validators.CategoryManagement;

namespace FunnyActivities.Application.UnitTests.Validators.CategoryManagement
{
    public class DeleteCategoryCommandValidatorTests
    {
        private readonly DeleteCategoryCommandValidator _validator;

        public DeleteCategoryCommandValidatorTests()
        {
            _validator = new DeleteCategoryCommandValidator();
        }

        [Fact]
        public void Validate_ValidCommand_ShouldPass()
        {
            // Arrange
            var command = new DeleteCategoryCommand
            {
                Id = Guid.NewGuid(),
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
            var command = new DeleteCategoryCommand
            {
                Id = Guid.Empty,
                UserId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Category ID is required.");
        }

        [Fact]
        public void Validate_EmptyUserId_ShouldFail()
        {
            // Arrange
            var command = new DeleteCategoryCommand
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Empty
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("User ID is required.");
        }

        [Fact]
        public void Validate_BothIdsEmpty_ShouldFail()
        {
            // Arrange
            var command = new DeleteCategoryCommand
            {
                Id = Guid.Empty,
                UserId = Guid.Empty
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id);
            result.ShouldHaveValidationErrorFor(x => x.UserId);
            Assert.Equal(2, result.Errors.Count);
        }
    }
}