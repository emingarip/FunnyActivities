using FluentValidation;
using FunnyActivities.Application.Commands.ProductVariantManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.ProductVariantManagement
{
    /// <summary>
    /// Validator for DeleteProductVariantCommand.
    /// </summary>
    public class DeleteProductVariantCommandValidator : AbstractValidator<DeleteProductVariantCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteProductVariantCommandValidator"/> class.
        /// </summary>
        public DeleteProductVariantCommandValidator()
        {
            // Id validation
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.Get("ProductVariantIdRequired"));

            // UserId validation
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.Get("UserIdRequired"));
        }
    }
}
