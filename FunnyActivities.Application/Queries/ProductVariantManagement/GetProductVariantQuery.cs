using MediatR;
using FunnyActivities.Application.DTOs.ProductVariantManagement;

namespace FunnyActivities.Application.Queries.ProductVariantManagement
{
    /// <summary>
    /// Query for retrieving a single product variant by ID.
    /// </summary>
    public class GetProductVariantQuery : IRequest<ProductVariantDto>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the product variant.
        /// </summary>
        public Guid Id { get; set; }
    }
}