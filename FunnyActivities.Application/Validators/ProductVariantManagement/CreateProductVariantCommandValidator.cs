using FluentValidation;
using FunnyActivities.Application.Commands.ProductVariantManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.ProductVariantManagement
{
    /// <summary>
    /// Validator for CreateProductVariantCommand.
    /// </summary>
    public class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateProductVariantCommandValidator"/> class.
        /// </summary>
        public CreateProductVariantCommandValidator()
        {
            // BaseProductId validation
            RuleFor(x => x.BaseProductId)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.Get("BaseProductIdRequired"));

            // Name validation
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.Get("NameRequired"))
                .MaximumLength(100)
                .WithMessage(ValidationMessageProvider.Get("NameMax100"));

            // StockQuantity validation
            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidationMessageProvider.Get("StockQuantityNonNegative"));

            // UnitOfMeasureId validation
            RuleFor(x => x.UnitOfMeasureId)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.Get("UnitOfMeasureIdRequired"));

            // UnitValue validation
            RuleFor(x => x.UnitValue)
                .GreaterThan(0)
                .WithMessage(ValidationMessageProvider.Get("UnitValuePositive"));

            // UsageNotes validation
            RuleFor(x => x.UsageNotes)
                .MaximumLength(300)
                .WithMessage(ValidationMessageProvider.Get("UsageNotesMax300"))
                .When(x => !string.IsNullOrEmpty(x.UsageNotes));

            // UserId validation
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage(ValidationMessageProvider.Get("UserIdRequired"));
        }
    }
}
