using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Interfaces
{
    /// <summary>
    /// Repository interface for ActivityCategory entities.
    /// </summary>
    public interface IActivityCategoryRepository
    {
        /// <summary>
        /// Gets an activity category by its ID.
        /// </summary>
        /// <param name="id">The activity category ID.</param>
        /// <returns>The activity category if found; otherwise, null.</returns>
        Task<ActivityCategory?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all activity categories.
        /// </summary>
        /// <returns>A list of all activity categories.</returns>
        Task<List<ActivityCategory>> GetAllAsync();

        /// <summary>
        /// Checks if an activity category exists by name.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>True if exists; otherwise, false.</returns>
        Task<bool> ExistsByNameAsync(string name);

        /// <summary>
        /// Checks if an activity category exists by name excluding a specific ID.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="excludeId">The ID to exclude from the check.</param>
        /// <returns>True if exists; otherwise, false.</returns>
        Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId);

        /// <summary>
        /// Adds a new activity category.
        /// </summary>
        /// <param name="category">The activity category to add.</param>
        Task AddAsync(ActivityCategory category);

        /// <summary>
        /// Updates an existing activity category.
        /// </summary>
        /// <param name="category">The activity category to update.</param>
        Task UpdateAsync(ActivityCategory category);

        /// <summary>
        /// Deletes an activity category.
        /// </summary>
        /// <param name="category">The activity category to delete.</param>
        Task DeleteAsync(ActivityCategory category);
    }
}