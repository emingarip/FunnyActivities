using System;
using MediatR;

namespace FunnyActivities.Domain.Events
{
    public class ActivityProductVariantCreatedEvent : IDomainEvent, INotification
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