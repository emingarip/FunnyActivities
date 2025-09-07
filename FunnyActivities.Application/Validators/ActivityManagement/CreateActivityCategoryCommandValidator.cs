using FluentValidation;
using FunnyActivities.Application.Commands.ActivityManagement;

namespace FunnyActivities.Application.Validators.ActivityManagement
{
    /// <summary>
    /// Validator for CreateActivityCategoryCommand.
    /// </summary>
    public class CreateActivityCategoryCommandValidator : AbstractValidator<CreateActivityCategoryCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateActivityCategoryCommandValidator"/> class.
        /// </summary>
        public CreateActivityCategoryCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .Length(1, 100).WithMessage("Category name must be between 1 and 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Category description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");
        }
    }
}