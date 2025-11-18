using FluentValidation;
using FunnyActivities.Application.Commands.PersonaManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.PersonaManagement
{
    public class CreatePersonaCommandValidator : AbstractValidator<CreatePersonaCommand>
    {
        public CreatePersonaCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("UserIdRequired"));

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("PersonaNameRequired"))
                .MaximumLength(100).WithMessage(ValidationMessageProvider.Get("PersonaNameMax100"))
                .Matches(@"^[a-zA-Z0-9\s\-_]+$").WithMessage(ValidationMessageProvider.Get("PersonaNameInvalidCharacters"));

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage(ValidationMessageProvider.Get("PersonaDescriptionMax500"));

            RuleFor(x => x.Age)
                .GreaterThan(0).WithMessage(ValidationMessageProvider.Get("PersonaAgePositive"))
                .When(x => x.Age.HasValue);

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage(ValidationMessageProvider.Get("PersonaGenderInvalid"))
                .When(x => x.Gender.HasValue);

            RuleFor(x => x.Nationality)
                .MaximumLength(100).WithMessage(ValidationMessageProvider.Get("PersonaNationalityMax100"));

            RuleFor(x => x.Biography)
                .MaximumLength(2000).WithMessage(ValidationMessageProvider.Get("PersonaBiographyMax2000"));

            RuleFor(x => x.AvatarImageUrl)
                .Must(BeValidUrl).WithMessage(ValidationMessageProvider.Get("PersonaAvatarUrlInvalid"))
                .When(x => !string.IsNullOrEmpty(x.AvatarImageUrl));

            RuleForEach(x => x.Characteristics).ChildRules(characteristic =>
            {
                characteristic.RuleFor(c => c.Name)
                    .NotEmpty().WithMessage(ValidationMessageProvider.Get("PersonaCharacteristicNameRequired"))
                    .MaximumLength(50).WithMessage(ValidationMessageProvider.Get("PersonaCharacteristicNameMax50"));

                characteristic.RuleFor(c => c.Value)
                    .NotEmpty().WithMessage(ValidationMessageProvider.Get("PersonaCharacteristicValueRequired"))
                    .MaximumLength(200).WithMessage(ValidationMessageProvider.Get("PersonaCharacteristicValueMax200"));

                characteristic.RuleFor(c => c.Order)
                    .GreaterThanOrEqualTo(0).WithMessage(ValidationMessageProvider.Get("PersonaCharacteristicOrderNonNegative"));
            });
        }

        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
