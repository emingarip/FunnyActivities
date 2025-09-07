using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Application.DTOs.UserManagement;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Domain.Services;
using FunnyActivities.Domain.ValueObjects;

namespace FunnyActivities.Application.Handlers.UserManagement
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResponse>
    {
        private readonly FunnyActivities.Domain.Interfaces.IUserRepository _userRepository;
        private readonly UserService _userService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<LoginUserCommandHandler> _logger;

        public LoginUserCommandHandler(
            FunnyActivities.Domain.Interfaces.IUserRepository userRepository,
            UserService userService,
            IJwtTokenService jwtTokenService,
            ILogger<LoginUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _userService = userService;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        public async Task<LoginResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("[LOGIN-HANDLER] Starting login process for email: {Email} at {Timestamp}",
                MaskEmail(request.Email), startTime);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                _logger.LogDebug("[LOGIN-HANDLER] Cancellation token check passed");

                _logger.LogInformation("[LOGIN-HANDLER] Processing login request for email: {Email}", MaskEmail(request.Email));

                // Database query timing
                var dbQueryStart = DateTime.UtcNow;
                _logger.LogDebug("[LOGIN-HANDLER] Starting database query for user lookup at {Timestamp}", dbQueryStart);

                var user = await _userRepository.GetByEmailAsync(request.Email).ConfigureAwait(false);

                var dbQueryEnd = DateTime.UtcNow;
                var dbQueryDuration = dbQueryEnd - dbQueryStart;
                _logger.LogInformation("[LOGIN-HANDLER] Database query completed in {Duration}ms. User found: {UserFound}",
                    dbQueryDuration.TotalMilliseconds, user != null);

                cancellationToken.ThrowIfCancellationRequested();
                _logger.LogDebug("[LOGIN-HANDLER] Second cancellation token check passed");

                // Password verification timing
                var passwordCheckStart = DateTime.UtcNow;
                _logger.LogDebug("[LOGIN-HANDLER] Starting password verification at {Timestamp}", passwordCheckStart);

                bool passwordValid = false;
                if (user != null)
                {
                    passwordValid = _userService.VerifyPassword(user.PasswordHash, request.Password);
                }

                var passwordCheckEnd = DateTime.UtcNow;
                var passwordCheckDuration = passwordCheckEnd - passwordCheckStart;
                _logger.LogInformation("[LOGIN-HANDLER] Password verification completed in {Duration}ms. Result: {PasswordValid}",
                    passwordCheckDuration.TotalMilliseconds, passwordValid);

                if (user == null || !passwordValid)
                {
                    _logger.LogWarning("[LOGIN-HANDLER] Login failed: Invalid credentials for email {Email}. User exists: {UserExists}",
                        MaskEmail(request.Email), user != null);
                    throw new UnauthorizedAccessException("Invalid email or password");
                }

                _logger.LogInformation("[LOGIN-HANDLER] User authentication successful for {UserId}", user.Id);

                // Password re-hashing check
                if (_userService.IsOldHashFormat(user.PasswordHash))
                {
                    _logger.LogInformation("[LOGIN-HANDLER] Old password format detected for user {UserId}, starting re-hash", user.Id);

                    var rehashStart = DateTime.UtcNow;
                    var newHash = _userService.HashPassword(new Password(request.Password));
                    user.SetPasswordHash(newHash);

                    var updateStart = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user).ConfigureAwait(false);
                    var updateEnd = DateTime.UtcNow;
                    var updateDuration = updateEnd - updateStart;

                    _logger.LogInformation("[LOGIN-HANDLER] Password re-hashed and updated for user {UserId} in {Duration}ms",
                        user.Id, updateDuration.TotalMilliseconds);
                }

                cancellationToken.ThrowIfCancellationRequested();
                _logger.LogDebug("[LOGIN-HANDLER] Third cancellation token check passed");

                var roleString = user.Role.ToString();
                _logger.LogInformation("[LOGIN-HANDLER] Generating JWT token for user {UserId} with role {Role}",
                    user.Id, user.Role);

                // JWT generation timing
                var jwtStart = DateTime.UtcNow;
                _logger.LogDebug("[LOGIN-HANDLER] Starting JWT token generation at {Timestamp}", jwtStart);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.GivenName, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName),
                    new Claim(ClaimTypes.Role, roleString),
                    new Claim("role", roleString) // Add both standard and custom role claim
                };

                _logger.LogDebug("[LOGIN-HANDLER] JWT claims created: {Claims}",
                    string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}")));

                var token = _jwtTokenService.GenerateToken(claims);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                var jwtEnd = DateTime.UtcNow;
                var jwtDuration = jwtEnd - jwtStart;
                _logger.LogInformation("[LOGIN-HANDLER] JWT token generation completed in {Duration}ms", jwtDuration.TotalMilliseconds);

                _logger.LogInformation("[LOGIN-HANDLER] Login successful for user {UserId}", user.Id);

                var endTime = DateTime.UtcNow;
                var totalDuration = endTime - startTime;
                _logger.LogInformation("[LOGIN-HANDLER] Login process completed successfully in {TotalDuration}ms for user {UserId}",
                    totalDuration.TotalMilliseconds, user.Id);

                return new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
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
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var totalDuration = endTime - startTime;
                _logger.LogError(ex, "[LOGIN-HANDLER] Login process failed after {TotalDuration}ms for email {Email}. Error: {ErrorMessage}",
                    totalDuration.TotalMilliseconds, MaskEmail(request.Email), ex.Message);
                throw;
            }
        }

        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return "***";
            var atIndex = email.IndexOf('@');
            if (atIndex > 3)
                return email.Substring(0, 3) + "***" + email.Substring(atIndex);
            return "***" + email.Substring(atIndex);
        }
    }
}