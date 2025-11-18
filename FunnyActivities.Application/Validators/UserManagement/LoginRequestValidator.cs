using FluentValidation;
using FunnyActivities.Application.DTOs.UserManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.UserManagement;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessageProvider.Get("EmailRequired"))
            .EmailAddress().WithMessage(ValidationMessageProvider.Get("InvalidEmailFormat"));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessageProvider.Get("PasswordRequired"));
    }
}
