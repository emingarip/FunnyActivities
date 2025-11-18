using FluentValidation;
using FunnyActivities.Application.Commands.BaseProductManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.BaseProductManagement
{
    /// <summary>
    /// Validator for DeleteBaseProductCommand.
    /// </summary>
    public class DeleteBaseProductCommandValidator : AbstractValidator<DeleteBaseProductCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteBaseProductCommandValidator"/> class.
        /// </summary>
        public DeleteBaseProductCommandValidator()
        {
            // Id validation
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.Get("BaseProductIdRequired"));

            // UserId validation
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.Get("UserIdRequired"));
        }
    }
}
