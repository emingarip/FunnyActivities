using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FunnyActivities.Domain.Events;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Represents a step in an activity.
    /// </summary>
    public class Step
    {
        /// <summary>
        /// Gets the unique identifier of the step.
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
        /// Gets the order of the step in the activity.
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets the description of the step.
        /// </summary>
        [Required]
        public string Description { get; private set; }

        /// <summary>
        /// Gets the timestamp in seconds for the step.
        /// </summary>
        public int? TimestampSeconds { get; private set; }

        /// <summary>
        /// Gets the duration in seconds for the step.
        /// </summary>
        public int? DurationSeconds { get; private set; }

        /// <summary>
        /// Gets the pause time in seconds for the step.
        /// </summary>
        public int? PauseTimeSeconds { get; private set; }

        /// <summary>
        /// Gets the media attachments for the step.
        /// </summary>
        public List<string> MediaAttachments { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the date and time when the step was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets the date and time when the step was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Gets the domain events.
        /// </summary>
        public List<IDomainEvent> DomainEvents { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Step"/> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="activityId">The activity ID.</param>
        /// <param name="order">The order of the step.</param>
        /// <param name="description">The description of the step.</param>
        /// <param name="timestampSeconds">The timestamp in seconds.</param>
        /// <param name="durationSeconds">The duration in seconds.</param>
        /// <param name="pauseTimeSeconds">The pause time in seconds.</param>
        /// <param name="mediaAttachments">The media attachments.</param>
        public Step(Guid id, Guid activityId, int order, string description, int? timestampSeconds = null, int? durationSeconds = null, int? pauseTimeSeconds = null, List<string>? mediaAttachments = null)
        {
            Id = id;
            ActivityId = activityId;
            Order = order;
            Description = description;
            TimestampSeconds = timestampSeconds ?? 0;
            DurationSeconds = durationSeconds ?? 0;
            PauseTimeSeconds = pauseTimeSeconds ?? 0;
            MediaAttachments = mediaAttachments ?? new List<string>();
            DomainEvents = new List<IDomainEvent>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private Step() { }

        /// <summary>
        /// Creates a new step instance.
        /// </summary>
        /// <param name="activityId">The activity ID.</param>
        /// <param name="order">The order of the step.</param>
        /// <param name="description">The description of the step.</param>
        /// <param name="timestampSeconds">The timestamp in seconds.</param>
        /// <param name="durationSeconds">The duration in seconds.</param>
        /// <param name="pauseTimeSeconds">The pause time in seconds.</param>
        /// <param name="mediaAttachments">The media attachments.</param>
        /// <returns>A new step instance.</returns>
        public static Step Create(Guid activityId, int order, string description, int? timestampSeconds = null, int? durationSeconds = null, int? pauseTimeSeconds = null, List<string>? mediaAttachments = null)
        {
            var step = new Step(Guid.NewGuid(), activityId, order, description, timestampSeconds, durationSeconds, pauseTimeSeconds, mediaAttachments);
            step.AddDomainEvent(new StepCreatedEvent(step.Id, description));
            return step;
        }

        /// <summary>
        /// Updates the details of the step.
        /// </summary>
        /// <param name="order">The new order.</param>
        /// <param name="description">The new description.</param>
        /// <param name="timestampSeconds">The new timestamp in seconds.</param>
        /// <param name="durationSeconds">The new duration in seconds.</param>
        /// <param name="pauseTimeSeconds">The new pause time in seconds.</param>
        /// <param name="mediaAttachments">The new media attachments.</param>
        public void UpdateDetails(int order, string description, int? timestampSeconds = null, int? durationSeconds = null, int? pauseTimeSeconds = null, List<string>? mediaAttachments = null)
        {
            Order = order;
            Description = description;
            TimestampSeconds = timestampSeconds ?? TimestampSeconds;
            DurationSeconds = durationSeconds ?? DurationSeconds;
            PauseTimeSeconds = pauseTimeSeconds ?? PauseTimeSeconds;
            MediaAttachments = mediaAttachments ?? MediaAttachments;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new StepUpdatedEvent(Id, description));
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