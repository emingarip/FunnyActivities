using FluentValidation;
using FunnyActivities.Application.Queries.BaseProductManagement;

namespace FunnyActivities.Application.Validators.BaseProductManagement
{
    /// <summary>
    /// Validator for GetBaseProductsQuery.
    /// </summary>
    public class GetBaseProductsQueryValidator : AbstractValidator<GetBaseProductsQuery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetBaseProductsQueryValidator"/> class.
        /// </summary>
        public GetBaseProductsQueryValidator()
        {
            // SearchTerm validation
            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .WithMessage("Search term cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));

            // CategoryId validation
            RuleFor(x => x.CategoryId)
                .Must(x => x == null || x != Guid.Empty)
                .WithMessage("CategoryId must be a valid GUID if provided.")
                .When(x => x.CategoryId.HasValue);

            // PageNumber validation
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0.");

            // PageSize validation
            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("Page size must be greater than 0.")
                .LessThanOrEqualTo(100)
                .WithMessage("Page size cannot exceed 100.");
        }
    }
}