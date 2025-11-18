using FluentValidation;
using FunnyActivities.Application.Queries.CategoryManagement;
using FunnyActivities.Application.Validators;

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
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("SearchTermRequired"))
                .MinimumLength(1).WithMessage(ValidationMessageProvider.Get("SearchTermMin1"))
                .MaximumLength(100).WithMessage(ValidationMessageProvider.Get("SearchTermMax100"));

            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage(ValidationMessageProvider.Get("PageNumberMin1"));

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage(ValidationMessageProvider.Get("PageSizeMin1"))
                .LessThanOrEqualTo(100).WithMessage(ValidationMessageProvider.Get("PageSizeMax100"));
        }
    }
}
