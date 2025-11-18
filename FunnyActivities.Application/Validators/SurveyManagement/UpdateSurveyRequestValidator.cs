using FluentValidation;
using FunnyActivities.Application.DTOs.SurveyManagement;
using FunnyActivities.Application.Validators;

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
                .MaximumLength(200).WithMessage(ValidationMessageProvider.Get("SurveyTitleMax200"))
                .MinimumLength(3).WithMessage(ValidationMessageProvider.Get("SurveyTitleMin3"))
                .When(x => !string.IsNullOrEmpty(x.Title));

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage(ValidationMessageProvider.Get("SurveyDescriptionMax1000"))
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.StartDate)
                .Must(date => date.HasValue && date.Value != default)
                .WithMessage(ValidationMessageProvider.Get("SurveyStartDateValid"))
                .When(x => x.StartDate.HasValue);

            RuleFor(x => x.EndDate)
                .Must((request, endDate) => !endDate.HasValue || !request.StartDate.HasValue || endDate.Value > request.StartDate.Value)
                .WithMessage(ValidationMessageProvider.Get("SurveyEndDateAfterStart"))
                .When(x => x.EndDate.HasValue && x.StartDate.HasValue);

            RuleFor(x => x.MaxParticipants)
                .GreaterThan(0).WithMessage(ValidationMessageProvider.Get("SurveyMaxParticipantsPositive"))
                .When(x => x.MaxParticipants.HasValue);

            RuleFor(x => x.ActivityIds)
                .Must(ids => ids == null || ids.Count == 0 || ids.All(id => id != Guid.Empty))
                .WithMessage(ValidationMessageProvider.Get("SurveyActivityIdInvalid"))
                .When(x => x.ActivityIds != null);

            RuleFor(x => x)
                .Must(request => request.IsValid())
                .WithMessage(ValidationMessageProvider.Get("SurveyUpdateRequestInvalid"));
        }
    }
}
