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