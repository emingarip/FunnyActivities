using System;
using MediatR;

namespace FunnyActivities.Domain.Events
{
    public class StepUpdatedEvent : IDomainEvent, INotification
    {
        public Guid StepId { get; }
        public string Description { get; }
        public DateTime OccurredOn { get; }

        public StepUpdatedEvent(Guid stepId, string description)
        {
            StepId = stepId;
            Description = description;
            OccurredOn = DateTime.UtcNow;
        }
    }
}