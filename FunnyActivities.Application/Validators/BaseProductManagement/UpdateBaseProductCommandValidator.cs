using FluentValidation;
using FunnyActivities.Application.Commands.BaseProductManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.BaseProductManagement
{
    /// <summary>
    /// Validator for UpdateBaseProductCommand.
    /// </summary>
    public class UpdateBaseProductCommandValidator : AbstractValidator<UpdateBaseProductCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateBaseProductCommandValidator"/> class.
        /// </summary>
        public UpdateBaseProductCommandValidator()
        {
            // Id validation
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.Get("BaseProductIdRequired"));

            // Name validation
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage(ValidationMessageProvider.Get("NameMax100"))
                .When(x => !string.IsNullOrEmpty(x.Name));

            // Description validation
            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage(ValidationMessageProvider.Get("DescriptionMax500"))
                .When(x => !string.IsNullOrEmpty(x.Description));

            // CategoryId validation
            RuleFor(x => x.CategoryId)
                .Must(x => x == null || x != Guid.Empty)
                .WithMessage(ValidationMessageProvider.Get("CategoryIdInvalid"))
                .When(x => x.CategoryId.HasValue);

            // UserId validation
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.Get("UserIdRequired"));
        }
    }
}
