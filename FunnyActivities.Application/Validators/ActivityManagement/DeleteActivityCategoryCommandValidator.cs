using FluentValidation;
using FunnyActivities.Application.Commands.ActivityManagement;

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
                .NotEmpty().WithMessage("Category ID is required.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");
        }
    }
}