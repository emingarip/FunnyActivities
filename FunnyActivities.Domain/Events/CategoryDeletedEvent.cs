using System;
using MediatR;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Events
{
    /// <summary>
    /// Event raised when a category is deleted.
    /// </summary>
    public class CategoryDeletedEvent : IDomainEvent, INotification
    {
        /// <summary>
        /// Gets the ID of the deleted category.
        /// </summary>
        public Guid CategoryId { get; }

        /// <summary>
        /// Gets the name of the deleted category.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the date and time when the event occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDeletedEvent"/> class.
        /// </summary>
        /// <param name="category">The deleted category.</param>
        public CategoryDeletedEvent(Category category)
        {
            CategoryId = category.Id;
            Name = category.Name;
            OccurredOn = DateTime.UtcNow;
        }
    }
}