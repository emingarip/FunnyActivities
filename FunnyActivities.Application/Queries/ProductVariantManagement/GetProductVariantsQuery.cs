using MediatR;
using FunnyActivities.Application.DTOs.ProductVariantManagement;
using FunnyActivities.Application.DTOs.Shared;

namespace FunnyActivities.Application.Queries.ProductVariantManagement
{
    /// <summary>
    /// Query for retrieving a list of product variants with optional filtering and pagination.
    /// </summary>
    public class GetProductVariantsQuery : IRequest<PagedResult<ProductVariantDto>>
    {
        /// <summary>
        /// Gets or sets the base product ID for filtering.
        /// </summary>
        public Guid? BaseProductId { get; set; }

        /// <summary>
        /// Gets or sets the search term for filtering product variants by name.
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure ID for filtering.
        /// </summary>
        public Guid? UnitOfMeasureId { get; set; }

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