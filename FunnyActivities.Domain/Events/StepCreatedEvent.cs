using System;

namespace FunnyActivities.Domain.Events
{
    public class StepCreatedEvent : IDomainEvent
    {
        public Guid StepId { get; }
        public string Description { get; }
        public DateTime OccurredOn { get; }

        public StepCreatedEvent(Guid stepId, string description)
        {
            StepId = stepId;
            Description = description;
            OccurredOn = DateTime.UtcNow;
        }
    }
}