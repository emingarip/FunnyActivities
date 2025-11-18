using FluentValidation;
using FunnyActivities.Application.Queries.ProductVariantManagement;
using FunnyActivities.Application.Validators;

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
                .WithMessage(ValidationMessageProvider.Get("PageNumberMin1"));

            // PageSize validation
            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage(ValidationMessageProvider.Get("PageSizeMin1"))
                .LessThanOrEqualTo(100)
                .WithMessage(ValidationMessageProvider.Get("PageSizeMax100"));

            // SearchTerm validation
            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .WithMessage(ValidationMessageProvider.Get("SearchTermMax100"))
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));
        }
    }
}
