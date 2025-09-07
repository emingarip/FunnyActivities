using System;

namespace FunnyActivities.Domain.Events
{
    public class ActivityCreatedEvent : IDomainEvent
    {
        public Guid ActivityId { get; }
        public string Name { get; }
        public DateTime OccurredOn { get; }

        public ActivityCreatedEvent(Guid activityId, string name)
        {
            ActivityId = activityId;
            Name = name;
            OccurredOn = DateTime.UtcNow;
        }
    }
}