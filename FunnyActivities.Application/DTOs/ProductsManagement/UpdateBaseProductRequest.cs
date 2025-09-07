using System;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Application.DTOs.ProductsManagement
{
    /// <summary>
    /// Request DTO for updating an existing base product.
    /// </summary>
    public class UpdateBaseProductRequest
    {
        /// <summary>
        /// Gets or sets the name of the base product.
        /// </summary>
        [StringLength(200, MinimumLength = 1)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the base product.
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the category ID of the base product.
        /// </summary>
        public Guid? CategoryId { get; set; }
    }
}