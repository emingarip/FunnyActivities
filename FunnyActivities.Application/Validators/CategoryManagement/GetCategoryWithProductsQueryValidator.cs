using FluentValidation;
using FunnyActivities.Application.Queries.CategoryManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.CategoryManagement
{
    /// <summary>
    /// Validator for GetCategoryWithProductsQuery.
    /// </summary>
    public class GetCategoryWithProductsQueryValidator : AbstractValidator<GetCategoryWithProductsQuery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetCategoryWithProductsQueryValidator"/> class.
        /// </summary>
        public GetCategoryWithProductsQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("CategoryIdRequired"));
        }
    }
}
