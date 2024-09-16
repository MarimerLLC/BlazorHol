using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace MauiBlazorHolAuth;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private AuthenticationState AuthenticationState { get; set; } =
        new AuthenticationState(new ClaimsPrincipal());

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(AuthenticationState);
    }

    public void SetPrincipal(ClaimsPrincipal principal)
    {
        AuthenticationState = new AuthenticationState(principal);
        NotifyAuthenticationStateChanged(Task.FromResult(AuthenticationState));
    }
}
