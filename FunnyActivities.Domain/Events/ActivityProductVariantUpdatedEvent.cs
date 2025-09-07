using System;

namespace FunnyActivities.Domain.Events
{
    public class ActivityProductVariantUpdatedEvent : IDomainEvent
    {
        public Guid ActivityProductVariantId { get; }
        public DateTime OccurredOn { get; }

        public ActivityProductVariantUpdatedEvent(Guid activityProductVariantId)
        {
            ActivityProductVariantId = activityProductVariantId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}