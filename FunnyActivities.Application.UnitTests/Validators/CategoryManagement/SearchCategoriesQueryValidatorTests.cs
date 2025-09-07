using FluentValidation.TestHelper;
using Xunit;
using FunnyActivities.Application.Queries.CategoryManagement;
using FunnyActivities.Application.Validators.CategoryManagement;

namespace FunnyActivities.Application.UnitTests.Validators.CategoryManagement
{
    public class SearchCategoriesQueryValidatorTests
    {
        private readonly SearchCategoriesQueryValidator _validator;

        public SearchCategoriesQueryValidatorTests()
        {
            _validator = new SearchCategoriesQueryValidator();
        }

        [Fact]
        public void Validate_ValidQuery_ShouldPass()
        {
            // Arrange
            var query = new SearchCategoriesQuery
            {
                SearchTerm = "test search",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_EmptySearchTerm_ShouldFail()
        {
            // Arrange
            var query = new SearchCategoriesQuery
            {
                SearchTerm = "",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SearchTerm)
                  .WithErrorMessage("Search term is required.");
        }

        [Fact]
        public void Validate_NullSearchTerm_ShouldFail()
        {
            // Arrange
            var query = new SearchCategoriesQuery
            {
                SearchTerm = null,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SearchTerm)
                  .WithErrorMessage("Search term is required.");
        }

        [Fact]
        public void Validate_SearchTermTooShort_ShouldFail()
        {
            // Arrange
            var query = new SearchCategoriesQuery
            {
                SearchTerm = "",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SearchTerm)
                  .WithErrorMessage("Search term is required.");
        }

        [Fact]
        public void Validate_SearchTermTooLong_ShouldFail()
        {
            // Arrange
            var query = new SearchCategoriesQuery
            {
                SearchTerm = new string('A', 101),
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SearchTerm)
                  .WithErrorMessage("Search term cannot exceed 100 characters.");
        }

        [Fact]
        public void Validate_PageNumberZero_ShouldFail()
        {
            // Arrange
            var query = new SearchCategoriesQuery
            {
                SearchTerm = "test",
                PageNumber = 0,
                PageSize = 10
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                  .WithErrorMessage("Page number must be greater than 0.");
        }

        [Fact]
        public void Validate_PageSizeZero_ShouldFail()
        {
            // Arrange
            var query = new SearchCategoriesQuery
            {
                SearchTerm = "test",
                PageNumber = 1,
                PageSize = 0
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                  .WithErrorMessage("Page size must be greater than 0.");
        }

        [Fact]
        public void Validate_PageSizeTooLarge_ShouldFail()
        {
            // Arrange
            var query = new SearchCategoriesQuery
            {
                SearchTerm = "test",
                PageNumber = 1,
                PageSize = 101
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                  .WithErrorMessage("Page size cannot exceed 100.");
        }
    }
}