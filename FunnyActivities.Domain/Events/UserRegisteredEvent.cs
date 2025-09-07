using System;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Events
{
    public class UserRegisteredEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public string Email { get; }
        public DateTime OccurredOn { get; }

        public UserRegisteredEvent(User user)
        {
            UserId = user.Id;
            Email = user.Email;
            OccurredOn = DateTime.UtcNow;
        }
    }
}