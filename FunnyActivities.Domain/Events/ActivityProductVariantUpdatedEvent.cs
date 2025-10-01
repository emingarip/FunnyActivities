using System;
using MediatR;

namespace FunnyActivities.Domain.Events
{
    public class ActivityProductVariantUpdatedEvent : IDomainEvent, INotification
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