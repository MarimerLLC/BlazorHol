namespace BlazorHolAuthentication.Policies;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class EuRequirement : IAuthorizationRequirement
{
    // List of EU countries (simplified for demonstration)
    public readonly string[] EuCountries = new[] { "DE", "FR", "IT", "ES", "NL", "BE", "PL", "SE", "AT" };
}


public class EuHandler : AuthorizationHandler<EuRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EuRequirement requirement)
    {
        // Check if the user has a claim indicating their region or country
        var countryClaim = context.User.FindFirst(
            claim => claim.Type == ClaimTypes.Country);

        if (countryClaim != null)
        {

            if (requirement.EuCountries.Contains(countryClaim.Value))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
