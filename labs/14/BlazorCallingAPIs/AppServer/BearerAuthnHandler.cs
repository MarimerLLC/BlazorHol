using Microsoft.AspNetCore.Authorization;

namespace AppServer;

public class BearerAuthnHandler(IHttpContextAccessor HttpContextAccessor) : AuthorizationHandler<BearerAuthnRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, BearerAuthnRequirement requirement)
    {
        if (HttpContextAccessor is null)
        {
            throw new ArgumentNullException(nameof(HttpContextAccessor));
        }
        var token = HttpContextAccessor.HttpContext?.Request.Headers.Authorization;
        if (string.IsNullOrWhiteSpace(token) || token.Value != "Bearer MyBearerTokenValue")
        {
            context.Fail();
        }
        else
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}

public class BearerAuthnRequirement : IAuthorizationRequirement
{
}
