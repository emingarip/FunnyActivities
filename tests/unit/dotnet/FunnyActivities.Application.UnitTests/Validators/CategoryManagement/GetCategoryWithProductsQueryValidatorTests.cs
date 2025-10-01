using System;
using FluentValidation.TestHelper;
using Xunit;
using FunnyActivities.Application.Queries.CategoryManagement;
using FunnyActivities.Application.Validators.CategoryManagement;

namespace FunnyActivities.Application.UnitTests.Validators.CategoryManagement
{
    public class GetCategoryWithProductsQueryValidatorTests
    {
        private readonly GetCategoryWithProductsQueryValidator _validator;

        public GetCategoryWithProductsQueryValidatorTests()
        {
            _validator = new GetCategoryWithProductsQueryValidator();
        }

        [Fact]
        public void Validate_ValidQuery_ShouldPass()
        {
            // Arrange
            var query = new GetCategoryWithProductsQuery
            {
                Id = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_EmptyId_ShouldFail()
        {
            // Arrange
            var query = new GetCategoryWithProductsQuery
            {
                Id = Guid.Empty
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Category ID is required.");
        }
    }
}