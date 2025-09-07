using System.Security.Claims;

namespace FunnyActivities.Domain.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(IEnumerable<Claim> claims);
        ClaimsPrincipal? ValidateToken(string token);
        string GenerateRefreshToken();
    }
}