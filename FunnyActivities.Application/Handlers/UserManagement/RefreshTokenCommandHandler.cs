using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Application.DTOs.UserManagement;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Domain.Services;

namespace FunnyActivities.Application.Handlers
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
    {
        private readonly FunnyActivities.Domain.Interfaces.IUserRepository _userRepository;
        private readonly UserService _userService;
        private readonly IJwtTokenService _jwtTokenService;

        public RefreshTokenCommandHandler(FunnyActivities.Domain.Interfaces.IUserRepository userRepository, UserService userService, IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Validate that we have both tokens
            if (string.IsNullOrEmpty(request.AccessToken) || string.IsNullOrEmpty(request.RefreshToken))
            {
                throw new UnauthorizedAccessException("Both access token and refresh token are required");
            }

            // Validate the access token to get user information
            var principal = _jwtTokenService.ValidateToken(request.AccessToken);
            if (principal == null)
            {
                // Add debugging information
                Console.WriteLine($"RefreshToken: Token validation failed for token: {request.AccessToken?.Substring(0, 50)}...");
                throw new UnauthorizedAccessException("Invalid access token");
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid access token");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            // For now, we'll accept any non-empty refresh token
            // In production, you'd validate the refresh token against stored tokens
            // TODO: Implement proper refresh token storage and validation

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = _jwtTokenService.GenerateToken(claims);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

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
    }
}