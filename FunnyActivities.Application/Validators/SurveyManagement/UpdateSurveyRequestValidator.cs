using FluentValidation;
using FunnyActivities.Application.DTOs.SurveyManagement;

namespace FunnyActivities.Application.Validators.SurveyManagement
{
    /// <summary>
    /// Validator for UpdateSurveyRequest.
    /// </summary>
    public class UpdateSurveyRequestValidator : AbstractValidator<UpdateSurveyRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateSurveyRequestValidator"/> class.
        /// </summary>
        public UpdateSurveyRequestValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage("Survey title cannot exceed 200 characters")
                .MinimumLength(3).WithMessage("Survey title must be at least 3 characters long")
                .When(x => !string.IsNullOrEmpty(x.Title));

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Survey description cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.StartDate)
                .Must(date => date.HasValue && date.Value != default)
                .WithMessage("Survey start date must be a valid date")
                .When(x => x.StartDate.HasValue);

            RuleFor(x => x.EndDate)
                .Must((request, endDate) => !endDate.HasValue || !request.StartDate.HasValue || endDate.Value > request.StartDate.Value)
                .WithMessage("Survey end date must be after start date")
                .When(x => x.EndDate.HasValue && x.StartDate.HasValue);

            RuleFor(x => x.MaxParticipants)
                .GreaterThan(0).WithMessage("Maximum participants must be greater than 0")
                .When(x => x.MaxParticipants.HasValue);

            RuleFor(x => x.ActivityIds)
                .Must(ids => ids == null || ids.Count == 0 || ids.All(id => id != Guid.Empty))
                .WithMessage("Activity IDs cannot be empty")
                .When(x => x.ActivityIds != null);

            RuleFor(x => x)
                .Must(request => request.IsValid())
                .WithMessage("Request contains invalid data - at least one field must be provided for update");
        }
    }
}