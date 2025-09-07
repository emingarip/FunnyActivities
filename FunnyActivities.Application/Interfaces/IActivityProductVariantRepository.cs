using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Interfaces
{
    /// <summary>
    /// Repository interface for ActivityProductVariant entities.
    /// </summary>
    public interface IActivityProductVariantRepository
    {
        /// <summary>
        /// Gets an activity product variant by its ID.
        /// </summary>
        /// <param name="id">The activity product variant ID.</param>
        /// <returns>The activity product variant if found; otherwise, null.</returns>
        Task<ActivityProductVariant?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets activity product variants by activity ID.
        /// </summary>
        /// <param name="activityId">The activity ID.</param>
        /// <returns>A list of activity product variants for the activity.</returns>
        Task<List<ActivityProductVariant>> GetByActivityIdAsync(Guid activityId);

        /// <summary>
        /// Gets activity product variants by product variant ID.
        /// </summary>
        /// <param name="productVariantId">The product variant ID.</param>
        /// <returns>A list of activity product variants for the product variant.</returns>
        Task<List<ActivityProductVariant>> GetByProductVariantIdAsync(Guid productVariantId);

        /// <summary>
        /// Adds a new activity product variant.
        /// </summary>
        /// <param name="activityProductVariant">The activity product variant to add.</param>
        Task AddAsync(ActivityProductVariant activityProductVariant);

        /// <summary>
        /// Updates an existing activity product variant.
        /// </summary>
        /// <param name="activityProductVariant">The activity product variant to update.</param>
        Task UpdateAsync(ActivityProductVariant activityProductVariant);

        /// <summary>
        /// Deletes an activity product variant.
        /// </summary>
        /// <param name="activityProductVariant">The activity product variant to delete.</param>
        Task DeleteAsync(ActivityProductVariant activityProductVariant);
    }
}