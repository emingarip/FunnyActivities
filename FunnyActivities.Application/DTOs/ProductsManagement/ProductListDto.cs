using System;
using System.Collections.Generic;
using FunnyActivities.Application.DTOs.ProductVariantManagement;

namespace FunnyActivities.Application.DTOs.ProductsManagement
{
    /// <summary>
    /// Data transfer object for product list information including variants.
    /// </summary>
    public class ProductListDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the product.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the product.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the category ID of the product.
        /// </summary>
        public Guid? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the category name of the product.
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the base price of the product.
        /// </summary>
        public decimal? BasePrice { get; set; }

        /// <summary>
        /// Gets or sets the stock status of the product.
        /// </summary>
        public string StockStatus { get; set; } = "out-of-stock";

        /// <summary>
        /// Gets or sets the total number of variants for this product.
        /// </summary>
        public int TotalVariants { get; set; }

        /// <summary>
        /// Gets or sets the list of product variants.
        /// </summary>
        public List<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();

        /// <summary>
        /// Gets or sets the creation date and time.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update date and time.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}