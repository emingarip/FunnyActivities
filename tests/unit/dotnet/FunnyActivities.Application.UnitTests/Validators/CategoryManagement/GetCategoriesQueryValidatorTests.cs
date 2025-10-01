using FluentValidation.TestHelper;
using Xunit;
using FunnyActivities.Application.Queries.CategoryManagement;
using FunnyActivities.Application.Validators.CategoryManagement;

namespace FunnyActivities.Application.UnitTests.Validators.CategoryManagement
{
    public class GetCategoriesQueryValidatorTests
    {
        private readonly GetCategoriesQueryValidator _validator;

        public GetCategoriesQueryValidatorTests()
        {
            _validator = new GetCategoriesQueryValidator();
        }

        [Fact]
        public void Validate_ValidQuery_ShouldPass()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "test",
                SortBy = "name",
                SortOrder = "asc"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_ValidQueryWithoutSearchTerm_ShouldPass()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = null,
                SortBy = null,
                SortOrder = null
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_PageNumberZero_ShouldFail()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
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
        public void Validate_PageNumberNegative_ShouldFail()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                PageNumber = -1,
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
            var query = new GetCategoriesQuery
            {
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
            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 101
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                  .WithErrorMessage("Page size cannot exceed 100.");
        }

        [Fact]
        public void Validate_PageSizeAtMaxLimit_ShouldPass()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 100
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
        }

        [Fact]
        public void Validate_InvalidSortBy_ShouldFail()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "invalid"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SortBy)
                  .WithErrorMessage("Invalid sort field. Valid values are: name, createdat.");
        }

        [Fact]
        public void Validate_ValidSortByName_ShouldPass()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "name"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
        }

        [Fact]
        public void Validate_ValidSortByCreatedAt_ShouldPass()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "createdat"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
        }

        [Fact]
        public void Validate_InvalidSortOrder_ShouldFail()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SortOrder = "invalid"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SortOrder)
                  .WithErrorMessage("Invalid sort order. Valid values are: asc, desc.");
        }

        [Fact]
        public void Validate_ValidSortOrderAsc_ShouldPass()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SortOrder = "asc"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SortOrder);
        }

        [Fact]
        public void Validate_ValidSortOrderDesc_ShouldPass()
        {
            // Arrange
            var query = new GetCategoriesQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SortOrder = "desc"
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SortOrder);
        }
    }
}