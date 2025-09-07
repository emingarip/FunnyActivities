using System;

namespace FunnyActivities.Domain.Events
{
    public class ActivityProductVariantCreatedEvent : IDomainEvent
    {
        public Guid ActivityProductVariantId { get; }
        public DateTime OccurredOn { get; }

        public ActivityProductVariantCreatedEvent(Guid activityProductVariantId)
        {
            ActivityProductVariantId = activityProductVariantId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}