using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Interfaces
{
    /// <summary>
    /// Repository interface for Activity entities.
    /// </summary>
    public interface IActivityRepository
    {
        /// <summary>
        /// Gets an activity by its ID.
        /// </summary>
        /// <param name="id">The activity ID.</param>
        /// <returns>The activity if found; otherwise, null.</returns>
        Task<Activity?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets activities by category ID with pagination.
        /// </summary>
        /// <param name="categoryId">The category ID.</param>
        /// <param name="page">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>A tuple with activities and total count.</returns>
        Task<(IEnumerable<Activity> Activities, int TotalCount)> GetByCategoryIdAsync(Guid categoryId, int page, int pageSize);

        /// <summary>
        /// Gets all activities.
        /// </summary>
        /// <returns>A list of all activities.</returns>
        Task<List<Activity>> GetAllAsync();

        /// <summary>
        /// Gets filtered and paginated activities.
        /// </summary>
        /// <param name="searchTerm">The search term for filtering by name or description.</param>
        /// <param name="activityCategoryId">The activity category ID for filtering.</param>
        /// <param name="isPublic">Whether to filter for public activities only.</param>
        /// <param name="sortBy">The field to sort by.</param>
        /// <param name="sortOrder">The sort order (asc or desc).</param>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A tuple with filtered activities and total count.</returns>
        Task<(IEnumerable<Activity> Activities, int TotalCount)> GetFilteredAsync(
            string? searchTerm,
            Guid? activityCategoryId,
            bool isPublic,
            string? sortBy,
            string? sortOrder,
            int pageNumber,
            int pageSize);

        /// <summary>
        /// Adds a new activity.
        /// </summary>
        /// <param name="activity">The activity to add.</param>
        Task AddAsync(Activity activity);

        /// <summary>
        /// Updates an existing activity.
        /// </summary>
        /// <param name="activity">The activity to update.</param>
        Task UpdateAsync(Activity activity);

        /// <summary>
        /// Deletes an activity.
        /// </summary>
        /// <param name="activity">The activity to delete.</param>
        Task DeleteAsync(Activity activity);
    }
}