using System;
using System.ComponentModel.DataAnnotations;
using FunnyActivities.Domain.Events;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents the association between an activity and a product variant.
    /// </summary>
    public class ActivityProductVariant
    {
        /// <summary>
        /// Gets the unique identifier of the activity product variant.
        /// </summary>
        [Key]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the activity ID.
        /// </summary>
        public Guid ActivityId { get; private set; }

        /// <summary>
        /// Gets the activity.
        /// </summary>
        public Activity Activity { get; private set; }

        /// <summary>
        /// Gets the product variant ID.
        /// </summary>
        public Guid ProductVariantId { get; private set; }

        /// <summary>
        /// Gets the product variant.
        /// </summary>
        public ProductVariant ProductVariant { get; private set; }

        /// <summary>
        /// Gets the quantity required for the activity.
        /// </summary>
        public decimal Quantity { get; private set; }

        /// <summary>
        /// Gets the unit of measure ID.
        /// </summary>
        public Guid UnitOfMeasureId { get; private set; }

        /// <summary>
        /// Gets the unit of measure.
        /// </summary>
        public UnitOfMeasure UnitOfMeasure { get; private set; }

        /// <summary>
        /// Gets the date and time when the association was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the date and time when the association was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Gets the domain events.
        /// </summary>
        public List<IDomainEvent> DomainEvents { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityProductVariant"/> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="activityId">The activity ID.</param>
        /// <param name="productVariantId">The product variant ID.</param>
        /// <param name="quantity">The quantity required.</param>
        /// <param name="unitOfMeasureId">The unit of measure ID.</param>
        public ActivityProductVariant(Guid id, Guid activityId, Guid productVariantId, decimal quantity, Guid unitOfMeasureId)
        {
            Id = id;
            ActivityId = activityId;
            ProductVariantId = productVariantId;
            Quantity = quantity;
            UnitOfMeasureId = unitOfMeasureId;
            DomainEvents = new List<IDomainEvent>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private ActivityProductVariant() { }

        /// <summary>
        /// Creates a new activity product variant instance.
        /// </summary>
        /// <param name="activityId">The activity ID.</param>
        /// <param name="productVariantId">The product variant ID.</param>
        /// <param name="quantity">The quantity required.</param>
        /// <param name="unitOfMeasureId">The unit of measure ID.</param>
        /// <returns>A new activity product variant instance.</returns>
        public static ActivityProductVariant Create(Guid activityId, Guid productVariantId, decimal quantity, Guid unitOfMeasureId)
        {
            var activityProductVariant = new ActivityProductVariant(Guid.NewGuid(), activityId, productVariantId, quantity, unitOfMeasureId);
            activityProductVariant.AddDomainEvent(new ActivityProductVariantCreatedEvent(activityProductVariant.Id));
            return activityProductVariant;
        }

        /// <summary>
        /// Updates the quantity and unit of measure.
        /// </summary>
        /// <param name="quantity">The new quantity.</param>
        /// <param name="unitOfMeasureId">The new unit of measure ID.</param>
        public void UpdateDetails(decimal quantity, Guid unitOfMeasureId)
        {
            Quantity = quantity;
            UnitOfMeasureId = unitOfMeasureId;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new ActivityProductVariantUpdatedEvent(Id));
        }

        /// <summary>
        /// Adds a domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event to add.</param>
        private void AddDomainEvent(IDomainEvent domainEvent)
        {
            DomainEvents ??= new List<IDomainEvent>();
            DomainEvents.Add(domainEvent);
        }

        /// <summary>
        /// Clears the domain events.
        /// </summary>
        public void ClearDomainEvents()
        {
            DomainEvents?.Clear();
        }
    }
}