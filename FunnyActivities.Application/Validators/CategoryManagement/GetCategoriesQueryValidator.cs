using FluentValidation;
using FunnyActivities.Application.Queries.CategoryManagement;

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
                .GreaterThan(0).WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");

            RuleFor(x => x.SortBy)
                .Must(BeValidSortBy).WithMessage("Invalid sort field. Valid values are: name, createdat.")
                .When(x => !string.IsNullOrEmpty(x.SortBy));

            RuleFor(x => x.SortOrder)
                .Must(BeValidSortOrder).WithMessage("Invalid sort order. Valid values are: asc, desc.")
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