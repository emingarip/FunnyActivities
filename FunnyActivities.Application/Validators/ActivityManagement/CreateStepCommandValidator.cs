using FluentValidation;
using FunnyActivities.Application.Commands.ActivityManagement;
using FunnyActivities.Application.Validators;

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
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("ActivityIdRequired"));

            RuleFor(x => x.Order)
                .GreaterThan(0).WithMessage(ValidationMessageProvider.Get("StepOrderGreaterThan0"));

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("StepDescriptionRequired"))
                .Length(1, 1000).WithMessage(ValidationMessageProvider.Get("StepDescriptionLength1To1000"));

            RuleFor(x => x.TimestampSeconds)
                .GreaterThanOrEqualTo(0).WithMessage(ValidationMessageProvider.Get("TimestampSecondsNonNegative"));

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("UserIdRequired"));
        }
    }
}
