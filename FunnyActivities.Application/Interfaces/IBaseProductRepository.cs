using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Interfaces
{
    /// <summary>
    /// Repository interface for BaseProduct entities.
    /// </summary>
    public interface IBaseProductRepository
    {
        /// <summary>
        /// Gets a base product by its ID.
        /// </summary>
        /// <param name="id">The base product ID.</param>
        /// <returns>The base product if found; otherwise, null.</returns>
        Task<BaseProduct?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets a paged list of base products with optional filtering.
        /// </summary>
        /// <param name="searchTerm">The search term for filtering by name or description.</param>
        /// <param name="categoryId">The category ID for filtering.</param>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>A list of base products.</returns>
        Task<List<BaseProduct>> GetPagedAsync(string? searchTerm, Guid? categoryId, int pageNumber, int pageSize);

        /// <summary>
        /// Gets a base product by name.
        /// </summary>
        /// <param name="name">The base product name.</param>
        /// <returns>The base product if found; otherwise, null.</returns>
        Task<BaseProduct?> GetByNameAsync(string name);

        /// <summary>
        /// Checks if a base product exists by name.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>True if exists; otherwise, false.</returns>
        Task<bool> ExistsByNameAsync(string name);

        /// <summary>
        /// Checks if a base product exists by name excluding a specific ID.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="excludeId">The ID to exclude from the check.</param>
        /// <returns>True if exists; otherwise, false.</returns>
        Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId);

        /// <summary>
        /// Adds a new base product.
        /// </summary>
        /// <param name="baseProduct">The base product to add.</param>
        Task AddAsync(BaseProduct baseProduct);

        /// <summary>
        /// Updates an existing base product.
        /// </summary>
        /// <param name="baseProduct">The base product to update.</param>
        Task UpdateAsync(BaseProduct baseProduct);

        /// <summary>
        /// Deletes a base product.
        /// </summary>
        /// <param name="baseProduct">The base product to delete.</param>
        Task DeleteAsync(BaseProduct baseProduct);
    }
}