using FluentValidation;
using FunnyActivities.Application.Commands.CategoryManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.CategoryManagement
{
    /// <summary>
    /// Validator for UpdateCategoryCommand.
    /// </summary>
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCategoryCommandValidator"/> class.
        /// </summary>
        public UpdateCategoryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("CategoryIdRequired"));

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
