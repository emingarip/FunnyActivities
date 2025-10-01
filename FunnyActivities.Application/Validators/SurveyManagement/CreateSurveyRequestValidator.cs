using FluentValidation;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Validators.SurveyManagement
{
    /// <summary>
    /// Validator for CreateSurveyRequest.
    /// </summary>
    public class CreateSurveyRequestValidator : AbstractValidator<CreateSurveyRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSurveyRequestValidator"/> class.
        /// </summary>
        public CreateSurveyRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Survey title is required")
                .MaximumLength(200).WithMessage("Survey title cannot exceed 200 characters")
                .MinimumLength(3).WithMessage("Survey title must be at least 3 characters long");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Survey description cannot exceed 1000 characters");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Survey start date is required")
                .Must(date => date != default).WithMessage("Survey start date must be a valid date")
                .Must(date => date >= DateTime.UtcNow.Date).WithMessage("Survey start date cannot be in the past");

            RuleFor(x => x.EndDate)
                .Must((request, endDate) => !endDate.HasValue || endDate.Value > request.StartDate)
                .WithMessage("Survey end date must be after start date")
                .When(x => x.EndDate.HasValue);

            RuleFor(x => x.MaxParticipants)
                .GreaterThan(0).WithMessage("Maximum participants must be greater than 0")
                .When(x => x.MaxParticipants.HasValue);

            RuleFor(x => x.ActivityIds)
                .NotEmpty().WithMessage("At least one activity is required")
                .Must(ids => ids != null && ids.Count > 0).WithMessage("At least one activity is required")
                .Must(ids => ids.All(id => id != Guid.Empty)).WithMessage("Activity IDs cannot be empty");

            RuleFor(x => x)
                .Must(request => request.IsValid())
                .WithMessage("Request contains invalid data");
        }
    }
}