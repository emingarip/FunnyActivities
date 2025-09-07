using FluentValidation;
using FunnyActivities.Application.Commands.ActivityManagement;

namespace FunnyActivities.Application.Validators.ActivityManagement
{
    /// <summary>
    /// Validator for CreateActivityCommand.
    /// </summary>
    public class CreateActivityCommandValidator : AbstractValidator<CreateActivityCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateActivityCommandValidator"/> class.
        /// </summary>
        public CreateActivityCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Activity name is required.")
                .Length(1, 200).WithMessage("Activity name must be between 1 and 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Activity description cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.VideoUrl)
                .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Invalid video URL format.")
                .When(x => !string.IsNullOrEmpty(x.VideoUrl));

            RuleFor(x => x.DurationHours)
                .InclusiveBetween(0, 23).WithMessage("Hours must be between 0 and 23.")
                .When(x => x.DurationHours.HasValue);

            RuleFor(x => x.DurationMinutes)
                .InclusiveBetween(0, 59).WithMessage("Minutes must be between 0 and 59.")
                .When(x => x.DurationMinutes.HasValue);

            RuleFor(x => x.DurationSeconds)
                .InclusiveBetween(0, 59).WithMessage("Seconds must be between 0 and 59.")
                .When(x => x.DurationSeconds.HasValue);

            RuleFor(x => x.ActivityCategoryId)
                .NotEmpty().WithMessage("Activity category ID is required.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");
        }
    }
}