using FluentValidation;
using FunnyActivities.Application.Queries.CategoryManagement;

namespace FunnyActivities.Application.Validators.CategoryManagement
{
    /// <summary>
    /// Validator for SearchCategoriesQuery.
    /// </summary>
    public class SearchCategoriesQueryValidator : AbstractValidator<SearchCategoriesQuery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCategoriesQueryValidator"/> class.
        /// </summary>
        public SearchCategoriesQueryValidator()
        {
            RuleFor(x => x.SearchTerm)
                .NotEmpty().WithMessage("Search term is required.")
                .MinimumLength(1).WithMessage("Search term must be at least 1 character long.")
                .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters.");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");
        }
    }
}