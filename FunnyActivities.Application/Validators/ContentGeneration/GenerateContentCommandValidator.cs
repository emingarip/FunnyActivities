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

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("ModelRequired"))
                .Must(BeValidModel).WithMessage(ValidationMessageProvider.Get("ModelInvalid"));
        }

        private bool BeValidModel(string model)
        {
            var validModels = new[] { "llama2", "codellama", "mistral", "vicuna" };
            return validModels.Contains(model.ToLower());
        }
    }
}
