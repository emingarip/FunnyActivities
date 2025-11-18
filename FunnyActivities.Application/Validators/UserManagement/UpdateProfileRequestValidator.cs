using FluentValidation;
using FunnyActivities.Application.DTOs.UserManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.UserManagement;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(ValidationMessageProvider.Get("FirstNameRequired"))
            .MaximumLength(50).WithMessage(ValidationMessageProvider.Get("FirstNameMax"));

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(ValidationMessageProvider.Get("LastNameRequired"))
            .MaximumLength(50).WithMessage(ValidationMessageProvider.Get("LastNameMax"));

        // ProfileImageUrl validation removed - handled via IFormFile in controller
    }
}
