using MediatR;

namespace FunnyActivities.Application.Commands.ProductVariantManagement
{
    /// <summary>
    /// Command for deleting a product variant.
    /// </summary>
    public class DeleteProductVariantCommand : IRequest<Unit>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the product variant to delete.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user deleting the variant.
        /// </summary>
        public Guid UserId { get; set; }
    }
}