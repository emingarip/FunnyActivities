using System;

namespace FunnyActivities.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to delete an activity category that still has activities.
    /// </summary>
    public class ActivityCategoryHasActivitiesException : Exception
    {
        /// <summary>
        /// Gets the ID of the category that still has activities.
        /// </summary>
        public Guid CategoryId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityCategoryHasActivitiesException"/> class.
        /// </summary>
        /// <param name="categoryId">The activity category ID.</param>
        public ActivityCategoryHasActivitiesException(Guid categoryId)
            : base($"Activity category with ID '{categoryId}' cannot be deleted because it still has activities.")
        {
            CategoryId = categoryId;
        }
    }
}
