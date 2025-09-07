using FluentValidation;
using FunnyActivities.Application.Queries.ProductVariantManagement;

namespace FunnyActivities.Application.Validators.ProductVariantManagement
{
    /// <summary>
    /// Validator for GetProductVariantsQuery.
    /// </summary>
    public class GetProductVariantsQueryValidator : AbstractValidator<GetProductVariantsQuery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetProductVariantsQueryValidator"/> class.
        /// </summary>
        public GetProductVariantsQueryValidator()
        {
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

            // SearchTerm validation
            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .WithMessage("Search term cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));
        }
    }
}