using FluentValidation;
using FunnyActivities.Application.Commands.BaseProductManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.BaseProductManagement
{
    /// <summary>
    /// Validator for CreateBaseProductCommand.
    /// </summary>
    public class CreateBaseProductCommandValidator : AbstractValidator<CreateBaseProductCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateBaseProductCommandValidator"/> class.
        /// </summary>
        public CreateBaseProductCommandValidator()
        {
            // Name validation
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("NameRequired"))
                .MaximumLength(100).WithMessage(ValidationMessageProvider.Get("NameMax100"));

            // Description validation
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage(ValidationMessageProvider.Get("DescriptionMax500"))
                .When(x => !string.IsNullOrEmpty(x.Description));

            // CategoryId validation
            RuleFor(x => x.CategoryId)
                .Must(x => x == null || x != Guid.Empty)
                .WithMessage(ValidationMessageProvider.Get("CategoryIdInvalid"))
                .When(x => x.CategoryId.HasValue);

            // UserId validation
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("UserIdRequired"));
        }
    }
}
