using FluentValidation;
using FunnyActivities.Application.Commands.ContentGeneration;

namespace FunnyActivities.Application.Validators.ContentGeneration
{
    public class GenerateContentCommandValidator : AbstractValidator<GenerateContentCommand>
    {
        public GenerateContentCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.PersonaId)
                .NotEmpty().WithMessage("Persona ID is required.");

            RuleFor(x => x.ActivityId)
                .NotEmpty().WithMessage("Activity ID is required.");

            RuleFor(x => x.CustomPrompt)
                .MaximumLength(1000).WithMessage("Custom prompt must not exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.CustomPrompt));

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model is required.")
                .Must(BeValidModel).WithMessage("Invalid model specified.");
        }

        private bool BeValidModel(string model)
        {
            var validModels = new[] { "llama2", "codellama", "mistral", "vicuna" };
            return validModels.Contains(model.ToLower());
        }
    }
}