using System;
using MediatR;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Events
{
    /// <summary>
    /// Event raised when a base product is created.
    /// </summary>
    public class BaseProductCreatedEvent : IDomainEvent, INotification
    {
        /// <summary>
        /// Gets the ID of the created base product.
        /// </summary>
        public Guid BaseProductId { get; }

        /// <summary>
        /// Gets the name of the created base product.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the date and time when the event occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProductCreatedEvent"/> class.
        /// </summary>
        /// <param name="baseProduct">The created base product.</param>
        public BaseProductCreatedEvent(BaseProduct baseProduct)
        {
            BaseProductId = baseProduct.Id;
            Name = baseProduct.Name;
            OccurredOn = DateTime.UtcNow;
        }
    }
}