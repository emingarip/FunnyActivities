using System;
using MediatR;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Events
{
    /// <summary>
    /// Event raised when a category is updated.
    /// </summary>
    public class CategoryUpdatedEvent : IDomainEvent, INotification
    {
        /// <summary>
        /// Gets the ID of the updated category.
        /// </summary>
        public Guid CategoryId { get; }

        /// <summary>
        /// Gets the name of the updated category.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the date and time when the event occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryUpdatedEvent"/> class.
        /// </summary>
        /// <param name="category">The updated category.</param>
        public CategoryUpdatedEvent(Category category)
        {
            CategoryId = category.Id;
            Name = category.Name;
            OccurredOn = DateTime.UtcNow;
        }
    }
}