using FluentValidation;
using FunnyActivities.Application.Commands.BaseProductManagement;

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
                .WithMessage("Base product ID is required.");

            // UserId validation
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");
        }
    }
}