using FluentValidation;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.Validators;

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
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("CategoryIdRequired"));

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("NameRequired"))
                .Length(1, 100).WithMessage(ValidationMessageProvider.Get("NameLength1To100"));

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage(ValidationMessageProvider.Get("DescriptionMax500"))
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("UserIdRequired"));
        }
    }
}
