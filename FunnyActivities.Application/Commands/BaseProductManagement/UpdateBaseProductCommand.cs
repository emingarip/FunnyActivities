using MediatR;
using FunnyActivities.Application.DTOs.BaseProductManagement;

namespace FunnyActivities.Application.Commands.BaseProductManagement
{
    /// <summary>
    /// Command for updating an existing base product.
    /// </summary>
    public class UpdateBaseProductCommand : IRequest<BaseProductDto>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the base product to update.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the base product.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the base product.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the category ID of the base product.
        /// </summary>
        public Guid? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user updating the base product.
        /// </summary>
        public Guid UserId { get; set; }
    }
}