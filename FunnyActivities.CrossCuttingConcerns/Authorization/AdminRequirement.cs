using Microsoft.AspNetCore.Authorization;

namespace FunnyActivities.CrossCuttingConcerns.Authorization;

public class AdminRequirement : IAuthorizationRequirement
{
    public string Role { get; }

    public AdminRequirement(string role)
    {
        Role = role;
    }
}