using FluentValidation;
using FunnyActivities.Application.Commands.CategoryManagement;
using FunnyActivities.Application.Validators;

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
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("CategoryNameRequired"))
                .Length(1, 100).WithMessage(ValidationMessageProvider.Get("CategoryNameLength1To100"));

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage(ValidationMessageProvider.Get("DescriptionMax500"))
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("UserIdRequired"));
        }
    }
}
