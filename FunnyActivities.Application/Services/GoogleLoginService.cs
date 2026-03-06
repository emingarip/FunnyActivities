using System.Security.Claims;
using System.Security.Cryptography;
using FunnyActivities.Application.DTOs.UserManagement;
using FunnyActivities.CrossCuttingConcerns.Authentication;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Domain.Services;
using FunnyActivities.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.Application.Services;

public class GoogleLoginService
{
    private readonly IUserRepository _userRepository;
    private readonly UserService _userService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IGoogleTokenVerifier _googleTokenVerifier;
    private readonly ILogger<GoogleLoginService> _logger;

    public GoogleLoginService(
        IUserRepository userRepository,
        UserService userService,
        IJwtTokenService jwtTokenService,
        IGoogleTokenVerifier googleTokenVerifier,
        ILogger<GoogleLoginService> logger)
    {
        _userRepository = userRepository;
        _userService = userService;
        _jwtTokenService = jwtTokenService;
        _googleTokenVerifier = googleTokenVerifier;
        _logger = logger;
    }

    public async Task<LoginResponse> AuthenticateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var googleUser = await _googleTokenVerifier.VerifyAsync(idToken, cancellationToken).ConfigureAwait(false);
        var normalizedEmail = Email.Normalize(googleUser.Email);
        var user = await _userRepository.GetByEmailAsync(normalizedEmail).ConfigureAwait(false);

        if (user == null)
        {
            user = new User(
                Guid.NewGuid(),
                normalizedEmail,
                CreateRandomPasswordHash(),
                ResolveFirstName(googleUser),
                ResolveLastName(googleUser),
                UserRole.User);

            user.UpdateLastLoginDate();
            await _userRepository.AddAsync(user).ConfigureAwait(false);

            _logger.LogInformation("[AUTH-GOOGLE] Created new account for {Email}", MaskEmail(normalizedEmail));
        }
        else
        {
            try
            {
                user.UpdateLastLoginDate();
                await _userRepository.UpdateAsync(user).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "[AUTH-GOOGLE] Failed to persist last login update for user {UserId}. Login will still succeed.",
                    user.Id);
            }

            _logger.LogInformation("[AUTH-GOOGLE] Reused existing account for {Email}", MaskEmail(normalizedEmail));
        }

        var roleString = user.Role.ToString();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new Claim(ClaimTypes.Role, roleString),
            new Claim("role", roleString)
        };

        return new LoginResponse
        {
            Token = _jwtTokenService.GenerateToken(claims),
            RefreshToken = _jwtTokenService.GenerateRefreshToken(),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfileImageUrl = user.ProfileImageUrl,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            }
        };
    }

    private string CreateRandomPasswordHash()
    {
        var randomPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        return _userService.HashPassword(new Password(randomPassword));
    }

    private static string ResolveFirstName(GoogleUserInfo googleUser)
    {
        if (!string.IsNullOrWhiteSpace(googleUser.GivenName))
        {
            return googleUser.GivenName;
        }

        var atIndex = googleUser.Email.IndexOf('@');
        return atIndex > 0 ? googleUser.Email[..atIndex] : "Google User";
    }

    private static string ResolveLastName(GoogleUserInfo googleUser)
    {
        return string.IsNullOrWhiteSpace(googleUser.FamilyName) ? string.Empty : googleUser.FamilyName;
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "***";
        }

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
        {
            return "***";
        }

        return atIndex > 3
            ? email[..3] + "***" + email[atIndex..]
            : "***" + email[atIndex..];
    }
}
