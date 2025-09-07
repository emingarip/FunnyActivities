using MediatR;

namespace FunnyActivities.Application.Commands.BaseProductManagement
{
    /// <summary>
    /// Command for deleting a base product.
    /// </summary>
    public class DeleteBaseProductCommand : IRequest<Unit>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the base product to delete.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user deleting the base product.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cascade delete variants.
        /// Defaults to false for backward compatibility.
        /// </summary>
        public bool CascadeDeleteVariants { get; set; } = false;
    }
}