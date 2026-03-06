using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FunnyActivities.CrossCuttingConcerns.Authentication;

internal static class JwtKeyHelper
{
    private const int MinimumKeyBytes = 32;

    public static SymmetricSecurityKey CreateSigningKey(string secretKey)
    {
        if (string.IsNullOrWhiteSpace(secretKey) || secretKey.StartsWith("__SET_IN_ENV", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");
        }

        var rawKeyBytes = Encoding.UTF8.GetBytes(secretKey);
        var effectiveKeyBytes = rawKeyBytes.Length >= MinimumKeyBytes
            ? rawKeyBytes
            : SHA256.HashData(rawKeyBytes);

        return new SymmetricSecurityKey(effectiveKeyBytes);
    }
}
