using System;
using MediatR;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Domain.Events
{
    public class UserRegisteredEvent : IDomainEvent, INotification
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