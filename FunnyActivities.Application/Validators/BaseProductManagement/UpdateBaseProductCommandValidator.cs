using FluentValidation;
using FunnyActivities.Application.Commands.BaseProductManagement;

namespace FunnyActivities.Application.Validators.BaseProductManagement
{
    /// <summary>
    /// Validator for UpdateBaseProductCommand.
    /// </summary>
    public class UpdateBaseProductCommandValidator : AbstractValidator<UpdateBaseProductCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateBaseProductCommandValidator"/> class.
        /// </summary>
        public UpdateBaseProductCommandValidator()
        {
            // Id validation
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Base product ID is required.");

            // Name validation
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage("Name cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.Name));

            // Description validation
            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            // CategoryId validation
            RuleFor(x => x.CategoryId)
                .Must(x => x == null || x != Guid.Empty)
                .WithMessage("CategoryId must be a valid GUID if provided.")
                .When(x => x.CategoryId.HasValue);

            // UserId validation
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");
        }
    }
}