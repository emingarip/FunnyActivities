using FluentValidation;
using FunnyActivities.Application.Queries.BaseProductManagement;
using FunnyActivities.Application.Validators;

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
                .WithMessage(ValidationMessageProvider.Get("SearchTermMax100"))
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));

            // CategoryId validation
            RuleFor(x => x.CategoryId)
                .Must(x => x == null || x != Guid.Empty)
                .WithMessage(ValidationMessageProvider.Get("CategoryIdInvalid"))
                .When(x => x.CategoryId.HasValue);

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
        }
    }
}
