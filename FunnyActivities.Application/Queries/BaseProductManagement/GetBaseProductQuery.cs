using MediatR;
using FunnyActivities.Application.DTOs.BaseProductManagement;

namespace FunnyActivities.Application.Queries.BaseProductManagement
{
    /// <summary>
    /// Query for retrieving a single base product by ID.
    /// </summary>
    public class GetBaseProductQuery : IRequest<BaseProductDto>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the base product.
        /// </summary>
        public Guid Id { get; set; }
    }
}