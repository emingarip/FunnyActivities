using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FunnyActivities.Domain.Events;
using FunnyActivities.Domain.ValueObjects;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents an activity.
    /// </summary>
    public class Activity
    {
        /// <summary>
        /// Gets the unique identifier of the activity.
        /// </summary>
        [Key]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the name of the activity.
        /// </summary>
        [Required]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the activity.
        /// </summary>
        public string? Description { get; private set; }

        /// <summary>
        /// Gets the video URL of the activity.
        /// </summary>
        public VideoUrl? VideoUrl { get; private set; }

        /// <summary>
        /// Gets the duration of the activity.
        /// </summary>
        public Duration? Duration { get; private set; }

        /// <summary>
        /// Gets the activity category ID.
        /// </summary>
        public Guid ActivityCategoryId { get; private set; }

        /// <summary>
        /// Gets the activity category.
        /// </summary>
        public ActivityCategory ActivityCategory { get; private set; }

        /// <summary>
        /// Gets the list of steps in the activity.
        /// </summary>
        public List<Step> Steps { get; private set; }

        /// <summary>
        /// Gets the list of activity product variants.
        /// </summary>
        public List<ActivityProductVariant> ActivityProductVariants { get; private set; }

        /// <summary>
        /// Gets the date and time when the activity was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the date and time when the activity was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Gets the domain events.
        /// </summary>
        public List<IDomainEvent> DomainEvents { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Activity"/> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="name">The name of the activity.</param>
        /// <param name="description">The description of the activity.</param>
        /// <param name="videoUrl">The video URL of the activity.</param>
        /// <param name="duration">The duration of the activity.</param>
        /// <param name="activityCategoryId">The activity category ID.</param>
        public Activity(Guid id, string name, string? description, VideoUrl? videoUrl, Duration? duration, Guid activityCategoryId)
        {
            Id = id;
            Name = name;
            Description = description;
            VideoUrl = videoUrl;
            Duration = duration;
            ActivityCategoryId = activityCategoryId;
            Steps = new List<Step>();
            ActivityProductVariants = new List<ActivityProductVariant>();
            DomainEvents = new List<IDomainEvent>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private Activity() { }

        /// <summary>
        /// Creates a new activity instance.
        /// </summary>
        /// <param name="name">The name of the activity.</param>
        /// <param name="description">The description of the activity.</param>
        /// <param name="videoUrl">The video URL of the activity.</param>
        /// <param name="duration">The duration of the activity.</param>
        /// <param name="activityCategoryId">The activity category ID.</param>
        /// <returns>A new activity instance.</returns>
        public static Activity Create(string name, string? description, VideoUrl? videoUrl, Duration? duration, Guid activityCategoryId)
        {
            var activity = new Activity(Guid.NewGuid(), name, description, videoUrl, duration, activityCategoryId);
            activity.AddDomainEvent(new ActivityCreatedEvent(activity.Id, name));
            return activity;
        }

        /// <summary>
        /// Updates the details of the activity.
        /// </summary>
        /// <param name="name">The new name.</param>
        /// <param name="description">The new description.</param>
        /// <param name="videoUrl">The new video URL.</param>
        /// <param name="duration">The new duration.</param>
        public void UpdateDetails(string name, string? description, VideoUrl? videoUrl, Duration? duration)
        {
            Name = name;
            Description = description;
            VideoUrl = videoUrl;
            Duration = duration;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new ActivityUpdatedEvent(Id, name));
        }

        /// <summary>
        /// Adds a step to the activity.
        /// </summary>
        /// <param name="step">The step to add.</param>
        public void AddStep(Step step)
        {
            Steps ??= new List<Step>();
            Steps.Add(step);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes a step from the activity.
        /// </summary>
        /// <param name="step">The step to remove.</param>
        public void RemoveStep(Step step)
        {
            Steps?.Remove(step);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds an activity product variant.
        /// </summary>
        /// <param name="activityProductVariant">The activity product variant to add.</param>
        public void AddProductVariant(ActivityProductVariant activityProductVariant)
        {
            ActivityProductVariants ??= new List<ActivityProductVariant>();
            ActivityProductVariants.Add(activityProductVariant);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes an activity product variant.
        /// </summary>
        /// <param name="activityProductVariant">The activity product variant to remove.</param>
        public void RemoveProductVariant(ActivityProductVariant activityProductVariant)
        {
            ActivityProductVariants?.Remove(activityProductVariant);
            UpdatedAt = DateTime.UtcNow;
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