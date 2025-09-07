using System;
using MediatR;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Events
{
    /// <summary>
    /// Event raised when a base product is updated.
    /// </summary>
    public class BaseProductUpdatedEvent : IDomainEvent, INotification
    {
        /// <summary>
        /// Gets the ID of the updated base product.
        /// </summary>
        public Guid BaseProductId { get; }

        /// <summary>
        /// Gets the name of the updated base product.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the date and time when the event occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProductUpdatedEvent"/> class.
        /// </summary>
        /// <param name="baseProduct">The updated base product.</param>
        public BaseProductUpdatedEvent(BaseProduct baseProduct)
        {
            BaseProductId = baseProduct.Id;
            Name = baseProduct.Name;
            OccurredOn = DateTime.UtcNow;
        }
    }
}