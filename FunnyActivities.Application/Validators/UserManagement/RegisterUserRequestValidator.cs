using FluentValidation;
using FunnyActivities.Application.DTOs.UserManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.UserManagement;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessageProvider.Get("EmailRequired"))
            .EmailAddress().WithMessage(ValidationMessageProvider.Get("InvalidEmailFormat"))
            .Must(BeValidEmail).WithMessage(ValidationMessageProvider.Get("EmailDomainInvalid"));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessageProvider.Get("PasswordRequired"))
            .MinimumLength(8).WithMessage(ValidationMessageProvider.Get("PasswordMinLength"))
            .Matches(@"[A-Z]").WithMessage(ValidationMessageProvider.Get("PasswordUppercase"))
            .Matches(@"[a-z]").WithMessage(ValidationMessageProvider.Get("PasswordLowercase"))
            .Matches(@"[0-9]").WithMessage(ValidationMessageProvider.Get("PasswordNumber"))
            .Matches(@"[\W]").WithMessage(ValidationMessageProvider.Get("PasswordSpecial"));

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(ValidationMessageProvider.Get("FirstNameRequired"))
            .MaximumLength(50).WithMessage(ValidationMessageProvider.Get("FirstNameMax"));

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(ValidationMessageProvider.Get("LastNameRequired"))
            .MaximumLength(50).WithMessage(ValidationMessageProvider.Get("LastNameMax"));
    }

    private bool BeValidEmail(string email)
    {
        // Custom rule: Email must not be from disposable domains
        var disposableDomains = new[] { "10minutemail.com", "temp-mail.org" };
        var domain = email.Split('@').Last();
        return !disposableDomains.Contains(domain);
    }
}
