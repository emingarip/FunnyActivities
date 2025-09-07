using FluentValidation;
using FunnyActivities.Application.DTOs.Shared;

namespace FunnyActivities.Application.Validators.UserManagement;

public class RequestPasswordResetRequestValidator : AbstractValidator<RequestPasswordResetRequest>
{
    public RequestPasswordResetRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}