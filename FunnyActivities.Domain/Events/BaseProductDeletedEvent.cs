using System;
using MediatR;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Events
{
    /// <summary>
    /// Event raised when a base product is deleted.
    /// </summary>
    public class BaseProductDeletedEvent : IDomainEvent, INotification
    {
        /// <summary>
        /// Gets the ID of the deleted base product.
        /// </summary>
        public Guid BaseProductId { get; }

        /// <summary>
        /// Gets the name of the deleted base product.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the date and time when the event occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProductDeletedEvent"/> class.
        /// </summary>
        /// <param name="baseProduct">The deleted base product.</param>
        public BaseProductDeletedEvent(BaseProduct baseProduct)
        {
            BaseProductId = baseProduct.Id;
            Name = baseProduct.Name;
            OccurredOn = DateTime.UtcNow;
        }
    }
}