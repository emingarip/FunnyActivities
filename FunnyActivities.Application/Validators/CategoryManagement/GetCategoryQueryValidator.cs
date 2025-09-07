using FluentValidation;
using FunnyActivities.Application.Queries.CategoryManagement;

namespace FunnyActivities.Application.Validators.CategoryManagement
{
    /// <summary>
    /// Validator for GetCategoryQuery.
    /// </summary>
    public class GetCategoryQueryValidator : AbstractValidator<GetCategoryQuery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetCategoryQueryValidator"/> class.
        /// </summary>
        public GetCategoryQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Category ID is required.");
        }
    }
}