using Microsoft.Extensions.Logging;
using System;

namespace FunnyActivities.CrossCuttingConcerns.Logging
{
    public class SecurityEventLogger
    {
        private readonly ILogger<SecurityEventLogger> _logger;

        public SecurityEventLogger(ILogger<SecurityEventLogger> logger)
        {
            _logger = logger;
        }

        public void LogAuthenticationSuccess(string userId, string email, string ipAddress, string userAgent, string correlationId)
        {
            _logger.LogInformation("Security Event: Authentication Success",
                new
                {
                    EventType = "AuthenticationSuccess",
                    UserId = userId,
                    Email = MaskEmail(email),
                    IPAddress = MaskIP(ipAddress),
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId
                });
        }

        public void LogAuthenticationFailure(string email, string ipAddress, string userAgent, string reason, string correlationId)
        {
            _logger.LogWarning("Security Event: Authentication Failure",
                new
                {
                    EventType = "AuthenticationFailure",
                    Email = MaskEmail(email),
                    IPAddress = MaskIP(ipAddress),
                    UserAgent = userAgent,
                    Reason = reason,
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId
                });
        }

        public void LogAuthorizationFailure(string userId, string resource, string action, string ipAddress, string correlationId)
        {
            _logger.LogWarning("Security Event: Authorization Failure",
                new
                {
                    EventType = "AuthorizationFailure",
                    UserId = userId,
                    Resource = resource,
                    Action = action,
                    IPAddress = MaskIP(ipAddress),
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId
                });
        }

        public void LogRateLimitExceeded(string ipAddress, string endpoint, int limit, string correlationId)
        {
            _logger.LogWarning("Security Event: Rate Limit Exceeded",
                new
                {
                    EventType = "RateLimitExceeded",
                    IPAddress = MaskIP(ipAddress),
                    Endpoint = endpoint,
                    Limit = limit,
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId
                });
        }

        public void LogSuspiciousActivity(string ipAddress, string activity, string details, string correlationId)
        {
            _logger.LogWarning("Security Event: Suspicious Activity",
                new
                {
                    EventType = "SuspiciousActivity",
                    IPAddress = MaskIP(ipAddress),
                    Activity = activity,
                    Details = details,
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId
                });
        }

        public void LogSecurityIncident(string incidentType, string severity, string description, string correlationId)
        {
            _logger.LogError("Security Event: Security Incident",
                new
                {
                    EventType = "SecurityIncident",
                    IncidentType = incidentType,
                    Severity = severity,
                    Description = description,
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId
                });
        }

        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return "***";
            var atIndex = email.IndexOf('@');
            if (atIndex > 3)
                return email.Substring(0, 3) + "***" + email.Substring(atIndex);
            return "***" + email.Substring(atIndex);
        }

        private string MaskIP(string ip)
        {
            if (string.IsNullOrEmpty(ip) || ip == "unknown") return ip;
            var parts = ip.Split('.');
            if (parts.Length >= 1)
                return parts[0] + ".***.***.***";
            return "***.***.***.***";
        }
    }
}