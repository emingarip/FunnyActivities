using System;
using MediatR;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Events
{
    /// <summary>
    /// Event raised when a step is deleted.
    /// </summary>
    public class StepDeletedEvent : IDomainEvent, INotification
    {
        /// <summary>
        /// Gets the ID of the deleted step.
        /// </summary>
        public Guid StepId { get; }

        /// <summary>
        /// Gets the description of the deleted step.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the date and time when the event occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepDeletedEvent"/> class.
        /// </summary>
        /// <param name="step">The deleted step.</param>
        public StepDeletedEvent(Step step)
        {
            StepId = step.Id;
            Description = step.Description;
            OccurredOn = DateTime.UtcNow;
        }
    }
}