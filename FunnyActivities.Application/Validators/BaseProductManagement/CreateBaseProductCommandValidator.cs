using FluentValidation;
using FunnyActivities.Application.Commands.BaseProductManagement;

namespace FunnyActivities.Application.Validators.BaseProductManagement
{
    /// <summary>
    /// Validator for CreateBaseProductCommand.
    /// </summary>
    public class CreateBaseProductCommandValidator : AbstractValidator<CreateBaseProductCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateBaseProductCommandValidator"/> class.
        /// </summary>
        public CreateBaseProductCommandValidator()
        {
            // Name validation
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(100)
                .WithMessage("Name cannot exceed 100 characters.");

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