using FluentValidation;
using FunnyActivities.Application.Commands.ContentGeneration;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.ContentGeneration
{
    public class GenerateContentCommandValidator : AbstractValidator<GenerateContentCommand>
    {
        public GenerateContentCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("UserIdRequired"));

            RuleFor(x => x.PersonaId)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("PersonaIdRequired"));

            RuleFor(x => x.ActivityId)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("ActivityIdRequired"));

            RuleFor(x => x.CustomPrompt)
                .MaximumLength(1000).WithMessage(ValidationMessageProvider.Get("CustomPromptMax1000"))
                .When(x => !string.IsNullOrEmpty(x.CustomPrompt));

            RuleFor(x => x.Provider)
                .IsInEnum().WithMessage(ValidationMessageProvider.Get("ProviderInvalid"))
                .When(x => x.Provider.HasValue);

            RuleFor(x => x.Model)
                .MaximumLength(100).WithMessage(ValidationMessageProvider.Get("ModelInvalid"))
                .When(x => !string.IsNullOrWhiteSpace(x.Model));

            RuleFor(x => x.Temperature)
                .InclusiveBetween(0f, 2f).WithMessage(ValidationMessageProvider.Get("TemperatureRange"))
                .When(x => x.Temperature.HasValue);

            RuleFor(x => x.MaxTokens)
                .GreaterThan(0).WithMessage(ValidationMessageProvider.Get("MaxTokensRange"))
                .When(x => x.MaxTokens.HasValue);
        }
    }
}
