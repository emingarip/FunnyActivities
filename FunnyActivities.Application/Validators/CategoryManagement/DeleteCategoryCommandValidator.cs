using FluentValidation;
using FunnyActivities.Application.Commands.CategoryManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.CategoryManagement
{
    /// <summary>
    /// Validator for DeleteCategoryCommand.
    /// </summary>
    public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteCategoryCommandValidator"/> class.
        /// </summary>
        public DeleteCategoryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("CategoryIdRequired"));

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(ValidationMessageProvider.Get("UserIdRequired"));
        }
    }
}
