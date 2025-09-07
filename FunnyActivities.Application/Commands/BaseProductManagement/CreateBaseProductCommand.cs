using MediatR;
using FunnyActivities.Application.DTOs.BaseProductManagement;

namespace FunnyActivities.Application.Commands.BaseProductManagement
{
    /// <summary>
    /// Command for creating a new base product.
    /// </summary>
    public class CreateBaseProductCommand : IRequest<BaseProductDto>
    {
        /// <summary>
        /// Gets or sets the name of the base product.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the base product.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the category ID of the base product.
        /// </summary>
        public Guid? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user creating the base product.
        /// </summary>
        public Guid UserId { get; set; }
    }
}