using FluentValidation;
using FunnyActivities.Application.Commands.ActivityManagement;

namespace FunnyActivities.Application.Validators.ActivityManagement
{
    /// <summary>
    /// Validator for UpdateActivityCategoryCommand.
    /// </summary>
    public class UpdateActivityCategoryCommandValidator : AbstractValidator<UpdateActivityCategoryCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateActivityCategoryCommandValidator"/> class.
        /// </summary>
        public UpdateActivityCategoryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Category ID is required.");

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