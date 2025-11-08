using System;
using System.Text.Json.Serialization;

namespace FunnyActivities.Domain.Entities
{
    /// <summary>
    /// Many-to-many join entity for activities and users.
    /// </summary>
    public class ActivityUser
    {
        public Guid ActivityId { get; private set; }

        [JsonIgnore]
        public Activity Activity { get; private set; }

        public Guid UserId { get; private set; }
        public User User { get; private set; }

        private ActivityUser() { }

        public ActivityUser(Guid activityId, Guid userId)
        {
            ActivityId = activityId;
            UserId = userId;
        }
    }
}
