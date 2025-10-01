using System;
using MediatR;

namespace FunnyActivities.Domain.Events
{
    public class ActivityCategoryUpdatedEvent : IDomainEvent, INotification
    {
        public Guid ActivityCategoryId { get; }
        public string Name { get; }
        public DateTime OccurredOn { get; }

        public ActivityCategoryUpdatedEvent(Guid activityCategoryId, string name)
        {
            ActivityCategoryId = activityCategoryId;
            Name = name;
            OccurredOn = DateTime.UtcNow;
        }
    }
}