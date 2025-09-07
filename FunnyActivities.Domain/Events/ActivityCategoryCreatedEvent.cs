using System;
using MediatR;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Events
{
    /// <summary>
    /// Event raised when an activity category is created.
    /// </summary>
    public class ActivityCategoryCreatedEvent : IDomainEvent, INotification
    {
        /// <summary>
        /// Gets the created activity category.
        /// </summary>
        public ActivityCategory ActivityCategory { get; }

        /// <summary>
        /// Gets the date and time when the event occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityCategoryCreatedEvent"/> class.
        /// </summary>
        /// <param name="activityCategory">The created activity category.</param>
        public ActivityCategoryCreatedEvent(ActivityCategory activityCategory)
        {
            ActivityCategory = activityCategory;
            OccurredOn = DateTime.UtcNow;
        }
    }
}