using System;
using MediatR;

namespace FunnyActivities.Domain.Events
{
    public class StepCreatedEvent : IDomainEvent, INotification
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