using System.Security.Claims;
using friasco_api.Enums;
using Microsoft.AspNetCore.Authorization;

namespace friasco_api.Helpers.AuthPolicies;

public class AdminOrSelfRequirement : IAuthorizationRequirement { }

public class AdminOrSelfHandler : AuthorizationHandler<AdminOrSelfRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOrSelfRequirement requirement)
    {
        var userClaims = context.User;
        var httpContext = context.Resource as HttpContext;
        var idParam = httpContext?.Request.RouteValues["id"];
        var userId = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userClaims.IsInRole(nameof(UserRoleEnum.Admin)) || 
            userClaims.IsInRole(nameof(UserRoleEnum.SuperAdmin)) ||
            (idParam?.ToString() == userId))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
