using FluentValidation;
using FunnyActivities.Application.DTOs.UserManagement;

namespace FunnyActivities.Application.Validators.UserManagement;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .Must(BeValidEmail).WithMessage("Email must be from a valid domain.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
            .Matches(@"[\W]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");
    }

    private bool BeValidEmail(string email)
    {
        // Custom rule: Email must not be from disposable domains
        var disposableDomains = new[] { "10minutemail.com", "temp-mail.org" };
        var domain = email.Split('@').Last();
        return !disposableDomains.Contains(domain);
    }
}