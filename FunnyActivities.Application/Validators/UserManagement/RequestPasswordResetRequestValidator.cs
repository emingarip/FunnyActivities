using FluentValidation;
using FunnyActivities.Application.DTOs.Shared;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.UserManagement;

public class RequestPasswordResetRequestValidator : AbstractValidator<RequestPasswordResetRequest>
{
    public RequestPasswordResetRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessageProvider.Get("EmailRequired"))
            .EmailAddress().WithMessage(ValidationMessageProvider.Get("InvalidEmailFormat"));
    }
}
