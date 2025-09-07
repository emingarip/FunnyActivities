using FluentValidation;
using FunnyActivities.Application.Commands.ProductVariantManagement;

namespace FunnyActivities.Application.Validators.ProductVariantManagement
{
    /// <summary>
    /// Validator for UpdateProductVariantCommand.
    /// </summary>
    public class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateProductVariantCommandValidator"/> class.
        /// </summary>
        public UpdateProductVariantCommandValidator()
        {
            // Id validation
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Product variant ID is required.");

            // Name validation
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(100)
                .WithMessage("Name cannot exceed 100 characters.");

            // UnitOfMeasureId validation
            RuleFor(x => x.UnitOfMeasureId)
                .NotEmpty()
                .WithMessage("Unit of measure ID is required.");

            // UnitValue validation
            RuleFor(x => x.UnitValue)
                .GreaterThan(0)
                .WithMessage("Unit value must be greater than zero.");

            // UsageNotes validation
            RuleFor(x => x.UsageNotes)
                .MaximumLength(300)
                .WithMessage("Usage notes cannot exceed 300 characters.")
                .When(x => !string.IsNullOrEmpty(x.UsageNotes));

            // UserId validation
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");
        }
    }
}