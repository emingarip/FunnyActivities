using System.Security.Cryptography;
using System.Text;

namespace FunnyActivities.CrossCuttingConcerns.Security;

public class DataAnonymizationService
{
    public string AnonymizeEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return email;

        var parts = email.Split('@');
        if (parts.Length != 2) return email;

        var localPart = parts[0];
        var domain = parts[1];

        // Keep first and last character, mask the middle
        if (localPart.Length > 2)
        {
            var masked = localPart[0] + new string('*', localPart.Length - 2) + localPart[^1];
            return $"{masked}@{domain}";
        }

        return $"***@{domain}";
    }

    public string AnonymizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber)) return phoneNumber;

        // Keep last 4 digits, mask the rest
        if (phoneNumber.Length > 4)
        {
            return new string('*', phoneNumber.Length - 4) + phoneNumber[^4..];
        }

        return new string('*', phoneNumber.Length);
    }

    public string HashData(string data)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(data);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public string PseudonymizeData(string data, string salt = "")
    {
        return HashData(data + salt);
    }
}