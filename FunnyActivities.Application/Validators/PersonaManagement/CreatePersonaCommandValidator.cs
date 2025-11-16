using FluentValidation;
using FunnyActivities.Application.Commands.PersonaManagement;

namespace FunnyActivities.Application.Validators.PersonaManagement
{
    public class CreatePersonaCommandValidator : AbstractValidator<CreatePersonaCommand>
    {
        public CreatePersonaCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Persona name is required.")
                .MaximumLength(100).WithMessage("Persona name must not exceed 100 characters.")
                .Matches(@"^[a-zA-Z0-9\s\-_]+$").WithMessage("Persona name contains invalid characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

            RuleFor(x => x.Age)
                .GreaterThan(0).WithMessage("Age must be a positive integer.")
                .When(x => x.Age.HasValue);

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Invalid gender value.");

            RuleFor(x => x.Nationality)
                .MaximumLength(100).WithMessage("Nationality must not exceed 100 characters.");

            RuleFor(x => x.Biography)
                .MaximumLength(2000).WithMessage("Biography must not exceed 2000 characters.");

            RuleFor(x => x.AvatarImageUrl)
                .Must(BeValidUrl).WithMessage("Avatar image URL must be a valid URL.")
                .When(x => !string.IsNullOrEmpty(x.AvatarImageUrl));

            RuleForEach(x => x.Characteristics).ChildRules(characteristic =>
            {
                characteristic.RuleFor(c => c.Name)
                    .NotEmpty().WithMessage("Characteristic name is required.")
                    .MaximumLength(50).WithMessage("Characteristic name must not exceed 50 characters.");

                characteristic.RuleFor(c => c.Value)
                    .NotEmpty().WithMessage("Characteristic value is required.")
                    .MaximumLength(200).WithMessage("Characteristic value must not exceed 200 characters.");

                characteristic.RuleFor(c => c.Order)
                    .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative.");
            });
        }

        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}