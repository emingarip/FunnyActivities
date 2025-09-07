using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using FunnyActivities.Domain.Interfaces;

namespace FunnyActivities.CrossCuttingConcerns.Authentication;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create a list of claims that includes the standard JWT claims
        var jwtClaims = new List<Claim>(claims);

        // Add standard JWT claims if they don't already exist
        if (!jwtClaims.Any(c => c.Type == JwtRegisteredClaimNames.Iss))
        {
            jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer));
        }
        if (!jwtClaims.Any(c => c.Type == JwtRegisteredClaimNames.Aud))
        {
            jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Aud, _jwtSettings.Audience));
        }
        if (!jwtClaims.Any(c => c.Type == JwtRegisteredClaimNames.Exp))
        {
            jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.Now.AddMinutes(_jwtSettings.ExpiryMinutes).ToUnixTimeSeconds().ToString()));
        }
        if (!jwtClaims.Any(c => c.Type == JwtRegisteredClaimNames.Iat))
        {
            jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: jwtClaims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds)
        {
            Header = { { "kid", "jwt-key-1" } } // Add key ID to JWT header
        };

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    public ClaimsPrincipal? ValidateToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var tokenHandler = new JwtSecurityTokenHandler();

        // Debug: Log the settings being used
        Console.WriteLine($"[DEBUG] JWT Settings - Issuer: {_jwtSettings.Issuer}, Audience: {_jwtSettings.Audience}, SecretKey length: {_jwtSettings.SecretKey.Length}");

        try
        {
            // Use the same validation parameters as the JWT Bearer middleware
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true, // Re-enable lifetime validation
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey))
            }, out var validatedToken);

            Console.WriteLine($"[DEBUG] JWT Validation successful");

            // Log the claims from the validated token
            if (validatedToken is JwtSecurityToken jwtToken)
            {
                var claims = jwtToken.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                Console.WriteLine($"[DEBUG] JWT Claims: {string.Join(", ", claims)}");

                // Specifically log role claims
                var roleClaims = jwtToken.Claims.Where(c => c.Type.Contains("role") || c.Type == ClaimTypes.Role).ToList();
                if (roleClaims.Any())
                {
                    Console.WriteLine($"[DEBUG] Role claims found: {string.Join(", ", roleClaims.Select(c => $"{c.Type}={c.Value}"))}");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] No role claims found in token");
                }
            }

            return principal;
        }
        catch (Exception ex)
        {
            // Add detailed debugging
            Console.WriteLine($"[DEBUG] JWT Validation failed: {ex.Message}");
            Console.WriteLine($"[DEBUG] Token: {token?.Substring(0, 50)}...");
            Console.WriteLine($"[DEBUG] SecretKey length: {_jwtSettings.SecretKey.Length}");
            Console.WriteLine($"[DEBUG] Issuer: {_jwtSettings.Issuer}");
            Console.WriteLine($"[DEBUG] Audience: {_jwtSettings.Audience}");

            // Try to decode the token to see its claims
            try
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);
                Console.WriteLine($"[DEBUG] Token Issuer: {jwtToken.Issuer}");
                Console.WriteLine($"[DEBUG] Token Audience: {jwtToken.Audiences.FirstOrDefault()}");
                Console.WriteLine($"[DEBUG] Token Expiry: {jwtToken.ValidTo}");
            }
            catch (Exception decodeEx)
            {
                Console.WriteLine($"[DEBUG] Failed to decode token: {decodeEx.Message}");
            }

            return null;
        }
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
    public int RefreshTokenExpiryDays { get; set; }
}