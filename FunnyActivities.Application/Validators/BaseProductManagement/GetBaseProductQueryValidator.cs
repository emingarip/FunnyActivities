using FluentValidation;
using FunnyActivities.Application.Queries.BaseProductManagement;

namespace FunnyActivities.Application.Validators.BaseProductManagement
{
    /// <summary>
    /// Validator for GetBaseProductQuery.
    /// </summary>
    public class GetBaseProductQueryValidator : AbstractValidator<GetBaseProductQuery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetBaseProductQueryValidator"/> class.
        /// </summary>
        public GetBaseProductQueryValidator()
        {
            // Id validation
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Base product ID is required.");
        }
    }
}