using System;
using MediatR;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Events
{
    /// <summary>
    /// Event raised when a category is created.
    /// </summary>
    public class CategoryCreatedEvent : IDomainEvent, INotification
    {
        /// <summary>
        /// Gets the ID of the created category.
        /// </summary>
        public Guid CategoryId { get; }

        /// <summary>
        /// Gets the name of the created category.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the date and time when the event occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryCreatedEvent"/> class.
        /// </summary>
        /// <param name="category">The created category.</param>
        public CategoryCreatedEvent(Category category)
        {
            CategoryId = category.Id;
            Name = category.Name;
            OccurredOn = DateTime.UtcNow;
        }
    }
}