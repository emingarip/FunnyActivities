using FluentValidation;
using FunnyActivities.Application.DTOs.SurveyManagement;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.SurveyManagement
{
    /// <summary>
    /// Validator for VoteRequest.
    /// </summary>
    public class VoteRequestValidator : AbstractValidator<VoteRequest>
    {
        private readonly ILogger<VoteRequestValidator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoteRequestValidator"/> class.
        /// </summary>
        public VoteRequestValidator(ILogger<VoteRequestValidator> logger)
        {
            _logger = logger;

            RuleFor(x => x.SurveyActivityId)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("SurveyActivityIdRequired"))
                .NotEqual(Guid.Empty).WithMessage(ValidationMessageProvider.Get("SurveyActivityIdInvalid"));

            RuleFor(x => x.VoteValue)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("VoteValueRequired"))
                .InclusiveBetween(1, 5).WithMessage(ValidationMessageProvider.Get("VoteValueRange"));

            // Comment is optional - no validation required

            RuleFor(x => x)
                .Must(request => request.IsValid())
                .WithMessage(ValidationMessageProvider.Get("RequestInvalid"));
        }

        public override FluentValidation.Results.ValidationResult Validate(ValidationContext<VoteRequest> context)
        {
            _logger.LogInformation("VoteRequestValidator.Validate called for SurveyActivityId: {SurveyActivityId}, VoteValue: {VoteValue}",
                context.InstanceToValidate.SurveyActivityId, context.InstanceToValidate.VoteValue);

            var result = base.Validate(context);

            if (!result.IsValid)
            {
                _logger.LogWarning("VoteRequest validation failed. Errors: {Errors}",
                    string.Join(", ", result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));
            }
            else
            {
                _logger.LogInformation("VoteRequest validation passed");
            }

            return result;
        }
    }
}
