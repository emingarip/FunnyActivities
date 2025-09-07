using System;

namespace FunnyActivities.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; private set; }
        public Guid? UserId { get; private set; }
        public string Action { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string IpAddress { get; private set; }
        public string UserAgent { get; private set; }
        public string Details { get; private set; }

        public AuditLog(Guid? userId, string action, string ipAddress, string userAgent, string details = null)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Action = action;
            Timestamp = DateTime.UtcNow;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            Details = details;
        }

        // Private constructor for EF or ORM
        private AuditLog() { }
    }
}