using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Interfaces
{
    /// <summary>
    /// Repository interface for ProductVariant entity operations.
    /// </summary>
    public interface IProductVariantRepository
    {
        /// <summary>
        /// Gets a product variant by its ID.
        /// </summary>
        /// <param name="id">The product variant ID.</param>
        /// <returns>The product variant if found; otherwise, null.</returns>
        Task<ProductVariant> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all product variants.
        /// </summary>
        /// <returns>A collection of all product variants.</returns>
        Task<IEnumerable<ProductVariant>> GetAllAsync();

        /// <summary>
        /// Gets product variants by base product ID.
        /// </summary>
        /// <param name="baseProductId">The base product ID.</param>
        /// <returns>A collection of product variants for the specified base product.</returns>
        Task<IEnumerable<ProductVariant>> GetByBaseProductIdAsync(Guid baseProductId);

        /// <summary>
        /// Adds a new product variant.
        /// </summary>
        /// <param name="productVariant">The product variant to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(ProductVariant productVariant);

        /// <summary>
        /// Updates an existing product variant.
        /// </summary>
        /// <param name="productVariant">The product variant to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(ProductVariant productVariant);

        /// <summary>
        /// Deletes a product variant.
        /// </summary>
        /// <param name="productVariant">The product variant to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAsync(ProductVariant productVariant);

        /// <summary>
        /// Checks if a product variant exists by its ID.
        /// </summary>
        /// <param name="id">The product variant ID.</param>
        /// <returns>True if the product variant exists; otherwise, false.</returns>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// Gets a product variant by name.
        /// </summary>
        /// <param name="name">The product variant name.</param>
        /// <returns>The product variant if found; otherwise, null.</returns>
        Task<ProductVariant> GetByNameAsync(string name);

        /// <summary>
        /// Gets a product variant by name and base product ID.
        /// </summary>
        /// <param name="name">The product variant name.</param>
        /// <param name="baseProductId">The base product ID.</param>
        /// <returns>The product variant if found; otherwise, null.</returns>
        Task<ProductVariant> GetByNameAndBaseProductAsync(string name, Guid baseProductId);
    }
}