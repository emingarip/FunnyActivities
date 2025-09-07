using FluentValidation;
using FunnyActivities.Application.Commands.CategoryManagement;

namespace FunnyActivities.Application.Validators.CategoryManagement
{
    /// <summary>
    /// Validator for CreateCategoryCommand.
    /// </summary>
    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCategoryCommandValidator"/> class.
        /// </summary>
        public CreateCategoryCommandValidator()
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