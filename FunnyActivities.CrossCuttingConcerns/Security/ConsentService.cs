using System.Collections.Concurrent;

namespace FunnyActivities.CrossCuttingConcerns.Security;

public class ConsentService
{
    private readonly ConcurrentDictionary<string, ConsentRecord> _consents = new();

    public void GrantConsent(string userId, string consentType, DateTime expiryDate)
    {
        var record = new ConsentRecord
        {
            UserId = userId,
            ConsentType = consentType,
            GrantedAt = DateTime.UtcNow,
            ExpiryDate = expiryDate,
            IsActive = true
        };

        _consents[$"{userId}:{consentType}"] = record;
    }

    public void RevokeConsent(string userId, string consentType)
    {
        var key = $"{userId}:{consentType}";
        if (_consents.TryGetValue(key, out var record))
        {
            record.IsActive = false;
            record.RevokedAt = DateTime.UtcNow;
        }
    }

    public bool HasConsent(string userId, string consentType)
    {
        var key = $"{userId}:{consentType}";
        if (_consents.TryGetValue(key, out var record))
        {
            return record.IsActive && record.ExpiryDate > DateTime.UtcNow;
        }
        return false;
    }

    public IEnumerable<ConsentRecord> GetUserConsents(string userId)
    {
        return _consents.Where(kvp => kvp.Key.StartsWith($"{userId}:")).Select(kvp => kvp.Value);
    }

    public class ConsentRecord
    {
        public string UserId { get; set; } = string.Empty;
        public string ConsentType { get; set; } = string.Empty;
        public DateTime GrantedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }
}