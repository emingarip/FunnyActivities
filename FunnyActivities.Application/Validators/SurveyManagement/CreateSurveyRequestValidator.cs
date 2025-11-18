using FluentValidation;
using FunnyActivities.Application.DTOs.SurveyManagement;
using FunnyActivities.Application.Validators;

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
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("SurveyTitleRequired"))
                .MaximumLength(200).WithMessage(ValidationMessageProvider.Get("SurveyTitleMax200"))
                .MinimumLength(3).WithMessage(ValidationMessageProvider.Get("SurveyTitleMin3"));

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage(ValidationMessageProvider.Get("SurveyDescriptionMax1000"));

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("SurveyStartDateRequired"))
                .Must(date => date != default).WithMessage(ValidationMessageProvider.Get("SurveyStartDateValid"))
                .Must(date => date >= DateTime.UtcNow.Date).WithMessage(ValidationMessageProvider.Get("SurveyStartDateNotPast"));

            RuleFor(x => x.EndDate)
                .Must((request, endDate) => !endDate.HasValue || endDate.Value > request.StartDate)
                .WithMessage(ValidationMessageProvider.Get("SurveyEndDateAfterStart"))
                .When(x => x.EndDate.HasValue);

            RuleFor(x => x.MaxParticipants)
                .GreaterThan(0).WithMessage(ValidationMessageProvider.Get("SurveyMaxParticipantsPositive"))
                .When(x => x.MaxParticipants.HasValue);

            RuleFor(x => x.ActivityIds)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("SurveyActivityRequired"))
                .Must(ids => ids != null && ids.Count > 0).WithMessage(ValidationMessageProvider.Get("SurveyActivityRequired"))
                .Must(ids => ids.All(id => id != Guid.Empty)).WithMessage(ValidationMessageProvider.Get("SurveyActivityIdInvalid"));

            RuleFor(x => x)
                .Must(request => request.IsValid())
                .WithMessage(ValidationMessageProvider.Get("RequestInvalid"));
        }
    }
}
