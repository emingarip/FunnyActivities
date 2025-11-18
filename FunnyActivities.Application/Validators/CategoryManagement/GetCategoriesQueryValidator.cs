using FluentValidation;
using FunnyActivities.Application.Queries.CategoryManagement;
using FunnyActivities.Application.Validators;

namespace FunnyActivities.Application.Validators.CategoryManagement
{
    /// <summary>
    /// Validator for GetCategoriesQuery.
    /// </summary>
    public class GetCategoriesQueryValidator : AbstractValidator<GetCategoriesQuery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetCategoriesQueryValidator"/> class.
        /// </summary>
        public GetCategoriesQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage(ValidationMessageProvider.Get("PageNumberMin1"));

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage(ValidationMessageProvider.Get("PageSizeMin1"))
                .LessThanOrEqualTo(100).WithMessage(ValidationMessageProvider.Get("PageSizeMax100"));

            RuleFor(x => x.SortBy)
                .Must(BeValidSortBy).WithMessage(ValidationMessageProvider.Get("SortByInvalid"))
                .When(x => !string.IsNullOrEmpty(x.SortBy));

            RuleFor(x => x.SortOrder)
                .Must(BeValidSortOrder).WithMessage(ValidationMessageProvider.Get("SortOrderInvalid"))
                .When(x => !string.IsNullOrEmpty(x.SortOrder));
        }

        private bool BeValidSortBy(string sortBy)
        {
            var validSortFields = new[] { "name", "createdat" };
            return validSortFields.Contains(sortBy.ToLower());
        }

        private bool BeValidSortOrder(string sortOrder)
        {
            var validSortOrders = new[] { "asc", "desc" };
            return validSortOrders.Contains(sortOrder.ToLower());
        }
    }
}
