using System;

namespace FunnyActivities.Domain.Events
{
    public class ActivityUpdatedEvent : IDomainEvent
    {
        public Guid ActivityId { get; }
        public string Name { get; }
        public DateTime OccurredOn { get; }

        public ActivityUpdatedEvent(Guid activityId, string name)
        {
            ActivityId = activityId;
            Name = name;
            OccurredOn = DateTime.UtcNow;
        }
    }
}