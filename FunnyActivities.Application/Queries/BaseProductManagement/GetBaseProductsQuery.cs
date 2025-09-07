using MediatR;
using FunnyActivities.Application.DTOs.BaseProductManagement;

namespace FunnyActivities.Application.Queries.BaseProductManagement
{
    /// <summary>
    /// Query for retrieving a list of base products with optional filtering and pagination.
    /// </summary>
    public class GetBaseProductsQuery : IRequest<List<BaseProductDto>>
    {
        /// <summary>
        /// Gets or sets the search term for filtering base products by name or description.
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the category ID for filtering base products.
        /// </summary>
        public Guid? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the page number for pagination (1-based).
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Gets or sets the page size for pagination.
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}