using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FunnyActivities.Domain.Events;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents a category for activities.
    /// </summary>
    public class ActivityCategory
    {
        /// <summary>
        /// Gets the unique identifier of the activity category.
        /// </summary>
        [Key]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        [Required]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the category.
        /// </summary>
        public string? Description { get; private set; }

        /// <summary>
        /// Gets the list of activities in this category.
        /// </summary>
        public List<Activity> Activities { get; private set; }

        /// <summary>
        /// Gets the date and time when the category was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the date and time when the category was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Gets the domain events.
        /// </summary>
        public List<IDomainEvent> DomainEvents { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityCategory"/> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="name">The name of the category.</param>
        /// <param name="description">The description of the category.</param>
        public ActivityCategory(Guid id, string name, string? description)
        {
            Id = id;
            Name = name;
            Description = description;
            Activities = new List<Activity>();
            DomainEvents = new List<IDomainEvent>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private ActivityCategory() { }

        /// <summary>
        /// Creates a new activity category instance.
        /// </summary>
        /// <param name="name">The name of the category.</param>
        /// <param name="description">The description of the category.</param>
        /// <returns>A new activity category instance.</returns>
        public static ActivityCategory Create(string name, string? description)
        {
            var category = new ActivityCategory(Guid.NewGuid(), name, description);
            category.AddDomainEvent(new ActivityCategoryCreatedEvent(category));
            return category;
        }

        /// <summary>
        /// Updates the details of the category.
        /// </summary>
        /// <param name="name">The new name.</param>
        /// <param name="description">The new description.</param>
        public void UpdateDetails(string name, string? description)
        {
            Name = name;
            Description = description;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new ActivityCategoryUpdatedEvent(Id, name));
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