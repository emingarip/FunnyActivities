using FluentValidation;
using FunnyActivities.Application.DTOs.UserManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.UserManagement;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(ValidationMessageProvider.Get("TokenRequired"));

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(ValidationMessageProvider.Get("NewPasswordRequired"))
            .MinimumLength(8).WithMessage(ValidationMessageProvider.Get("PasswordMinLength"))
            .Matches(@"[A-Z]").WithMessage(ValidationMessageProvider.Get("PasswordUppercase"))
            .Matches(@"[a-z]").WithMessage(ValidationMessageProvider.Get("PasswordLowercase"))
            .Matches(@"[0-9]").WithMessage(ValidationMessageProvider.Get("PasswordNumber"))
            .Matches(@"[\W]").WithMessage(ValidationMessageProvider.Get("PasswordSpecial"));
    }
}
