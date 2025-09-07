using FluentValidation;
using FunnyActivities.Application.DTOs.UserManagement;

namespace FunnyActivities.Application.Validators.UserManagement;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

        // ProfileImageUrl validation removed - handled via IFormFile in controller
    }
}