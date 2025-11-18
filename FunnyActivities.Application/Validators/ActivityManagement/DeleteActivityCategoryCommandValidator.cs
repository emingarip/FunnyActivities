using FluentValidation;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.ActivityManagement
{
    /// <summary>
    /// Validator for DeleteActivityCategoryCommand.
    /// </summary>
    public class DeleteActivityCategoryCommandValidator : AbstractValidator<DeleteActivityCategoryCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteActivityCategoryCommandValidator"/> class.
        /// </summary>
        public DeleteActivityCategoryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("CategoryIdRequired"));

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("UserIdRequired"));
        }
    }
}
