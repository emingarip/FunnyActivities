using System;

namespace FunnyActivities.Domain.Events
{
    public class StepUpdatedEvent : IDomainEvent
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