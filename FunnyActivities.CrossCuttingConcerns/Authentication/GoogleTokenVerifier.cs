using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FunnyActivities.CrossCuttingConcerns.Authentication;

public interface IGoogleTokenVerifier
{
    Task<GoogleUserInfo> VerifyAsync(string idToken, CancellationToken cancellationToken = default);
}

public sealed class GoogleAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
}

public sealed class GoogleUserInfo
{
    public string Email { get; init; } = string.Empty;
    public string GivenName { get; init; } = string.Empty;
    public string FamilyName { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string PictureUrl { get; init; } = string.Empty;
}

public sealed class GoogleTokenVerifier : IGoogleTokenVerifier
{
    private readonly GoogleAuthSettings _settings;
    private readonly ILogger<GoogleTokenVerifier> _logger;

    public GoogleTokenVerifier(
        IOptions<GoogleAuthSettings> settings,
        ILogger<GoogleTokenVerifier> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<GoogleUserInfo> VerifyAsync(string idToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(_settings.ClientId))
        {
            throw new InvalidOperationException("Google login is not configured.");
        }

        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new UnauthorizedAccessException("Google ID token is required.");
        }

        GoogleJsonWebSignature.Payload payload;

        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _settings.ClientId }
                }).ConfigureAwait(false);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "[AUTH-GOOGLE] Invalid Google ID token.");
            throw new UnauthorizedAccessException("Google ID token is invalid.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AUTH-GOOGLE] Google token verification failed.");
            throw new InvalidOperationException("Google login is temporarily unavailable.", ex);
        }

        if (payload.EmailVerified != true || string.IsNullOrWhiteSpace(payload.Email))
        {
            throw new UnauthorizedAccessException("Google account email is not verified.");
        }

        return new GoogleUserInfo
        {
            Email = payload.Email,
            GivenName = payload.GivenName ?? string.Empty,
            FamilyName = payload.FamilyName ?? string.Empty,
            Subject = payload.Subject ?? string.Empty,
            PictureUrl = payload.Picture ?? string.Empty
        };
    }
}
