using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Interfaces
{
    /// <summary>
    /// Repository interface for Category entities.
    /// </summary>
    public interface ICategoryRepository
    {
        /// <summary>
        /// Gets a category by its ID.
        /// </summary>
        /// <param name="id">The category ID.</param>
        /// <returns>The category if found; otherwise, null.</returns>
        Task<Category?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets a category by its name.
        /// </summary>
        /// <param name="name">The category name.</param>
        /// <returns>The category if found; otherwise, null.</returns>
        Task<Category?> GetByNameAsync(string name);

        /// <summary>
        /// Gets all categories.
        /// </summary>
        /// <returns>A list of all categories.</returns>
        Task<List<Category>> GetAllAsync();

        /// <summary>
        /// Checks if a category exists by name.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>True if exists; otherwise, false.</returns>
        Task<bool> ExistsByNameAsync(string name);

        /// <summary>
        /// Checks if a category exists by name excluding a specific ID.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="excludeId">The ID to exclude from the check.</param>
        /// <returns>True if exists; otherwise, false.</returns>
        Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId);

        /// <summary>
        /// Adds a new category.
        /// </summary>
        /// <param name="category">The category to add.</param>
        Task AddAsync(Category category);

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="category">The category to update.</param>
        Task UpdateAsync(Category category);

        /// <summary>
        /// Deletes a category.
        /// </summary>
        /// <param name="category">The category to delete.</param>
        Task DeleteAsync(Category category);
    }
}