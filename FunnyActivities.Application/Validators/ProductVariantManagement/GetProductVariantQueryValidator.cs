using FluentValidation;
using FunnyActivities.Application.Queries.ProductVariantManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.ProductVariantManagement
{
    /// <summary>
    /// Validator for GetProductVariantQuery.
    /// </summary>
    public class GetProductVariantQueryValidator : AbstractValidator<GetProductVariantQuery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetProductVariantQueryValidator"/> class.
        /// </summary>
        public GetProductVariantQueryValidator()
        {
            // Id validation
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.Get("ProductVariantIdRequired"));
        }
    }
}
