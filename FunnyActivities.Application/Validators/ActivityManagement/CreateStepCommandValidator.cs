using FluentValidation;
using FunnyActivities.Application.Commands.ActivityManagement;

namespace FunnyActivities.Application.Validators.ActivityManagement
{
    /// <summary>
    /// Validator for CreateStepCommand.
    /// </summary>
    public class CreateStepCommandValidator : AbstractValidator<CreateStepCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateStepCommandValidator"/> class.
        /// </summary>
        public CreateStepCommandValidator()
        {
            RuleFor(x => x.ActivityId)
                .NotEmpty().WithMessage("Activity ID is required.");

            RuleFor(x => x.Order)
                .GreaterThan(0).WithMessage("Order must be greater than 0.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .Length(1, 1000).WithMessage("Description must be between 1 and 1000 characters.");

            RuleFor(x => x.TimestampSeconds)
                .GreaterThanOrEqualTo(0).WithMessage("Timestamp seconds must be non-negative.")
                .When(x => x.TimestampSeconds.HasValue);

            RuleFor(x => x.DurationSeconds)
                .GreaterThanOrEqualTo(0).WithMessage("Duration seconds must be non-negative.")
                .When(x => x.DurationSeconds.HasValue);

            RuleFor(x => x.PauseTimeSeconds)
                .GreaterThanOrEqualTo(0).WithMessage("Pause time seconds must be non-negative.")
                .When(x => x.PauseTimeSeconds.HasValue);

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");
        }
    }
}