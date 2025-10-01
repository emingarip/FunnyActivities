using System;
using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents a user's favorite activity.
    /// </summary>
    public class Favorites
    {
        /// <summary>
        /// Gets the unique identifier of the favorite entry.
        /// </summary>
        [Key]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the user ID who favorited the activity.
        /// </summary>
        [Required]
        public Guid UserId { get; private set; }

        /// <summary>
        /// Gets the user who favorited the activity.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// Gets the activity ID that was favorited.
        /// </summary>
        [Required]
        public Guid ActivityId { get; private set; }

        /// <summary>
        /// Gets the activity that was favorited.
        /// </summary>
        public Activity Activity { get; private set; }

        /// <summary>
        /// Gets the date and time when the activity was favorited.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the date and time when the favorite was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Favorites"/> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="userId">The user ID who favorited the activity.</param>
        /// <param name="activityId">The activity ID that was favorited.</param>
        public Favorites(Guid id, Guid userId, Guid activityId)
        {
            Id = id;
            UserId = userId;
            ActivityId = activityId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private Favorites() { }

        /// <summary>
        /// Creates a new favorites instance.
        /// </summary>
        /// <param name="userId">The user ID who favorited the activity.</param>
        /// <param name="activityId">The activity ID that was favorited.</param>
        /// <returns>A new favorites instance.</returns>
        public static Favorites Create(Guid userId, Guid activityId)
        {
            return new Favorites(Guid.NewGuid(), userId, activityId);
        }

        /// <summary>
        /// Updates the timestamp when the favorite is modified.
        /// </summary>
        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}