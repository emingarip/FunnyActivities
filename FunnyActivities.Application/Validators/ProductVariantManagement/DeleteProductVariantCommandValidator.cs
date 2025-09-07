using FluentValidation;
using FunnyActivities.Application.Commands.ProductVariantManagement;

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
                .WithMessage("Product variant ID is required.");

            // UserId validation
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");
        }
    }
}