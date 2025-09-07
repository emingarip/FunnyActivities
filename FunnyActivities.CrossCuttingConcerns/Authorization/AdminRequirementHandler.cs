using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FunnyActivities.CrossCuttingConcerns.Authorization;

public class AdminRequirementHandler : AuthorizationHandler<AdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == requirement.Role))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}