using System;

namespace FunnyActivities.Application.DTOs.BaseProductManagement
{
    /// <summary>
    /// Data transfer object for base product information.
    /// </summary>
    public class BaseProductDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the base product.
        /// </summary>
        public Guid Id { get; set; }

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
        /// Gets or sets the category name of the base product.
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the base product was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the base product was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}