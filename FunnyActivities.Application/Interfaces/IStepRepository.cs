using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Interfaces
{
    /// <summary>
    /// Repository interface for Step entities.
    /// </summary>
    public interface IStepRepository
    {
        /// <summary>
        /// Gets a step by its ID.
        /// </summary>
        /// <param name="id">The step ID.</param>
        /// <returns>The step if found; otherwise, null.</returns>
        Task<Step?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets steps by activity ID.
        /// </summary>
        /// <param name="activityId">The activity ID.</param>
        /// <returns>A list of steps for the activity.</returns>
        Task<List<Step>> GetByActivityIdAsync(Guid activityId);

        /// <summary>
        /// Adds a new step.
        /// </summary>
        /// <param name="step">The step to add.</param>
        Task AddAsync(Step step);

        /// <summary>
        /// Updates an existing step.
        /// </summary>
        /// <param name="step">The step to update.</param>
        Task UpdateAsync(Step step);

        /// <summary>
        /// Deletes a step.
        /// </summary>
        /// <param name="step">The step to delete.</param>
        Task DeleteAsync(Step step);
    }
}