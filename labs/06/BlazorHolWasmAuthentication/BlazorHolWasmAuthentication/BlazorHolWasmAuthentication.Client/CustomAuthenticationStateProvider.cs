using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace BlazorHolWasmAuthentication.Client;

public class CustomAuthenticationStateProvider(HttpClient HttpClient) : AuthenticationStateProvider
{
    private AuthenticationState AuthenticationState { get; set; } =
        new AuthenticationState(new ClaimsPrincipal());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var previousUser = AuthenticationState.User;
        var user = await HttpClient.GetFromJsonAsync<User>("auth");
        if (user != null && !string.IsNullOrEmpty(user.Username))
        {
            var claims = new List<System.Security.Claims.Claim>();
            foreach (var claim in user.Claims)
            {
                claims.Add(new System.Security.Claims.Claim(claim.Type, claim.Value));
            }
            var identity = new ClaimsIdentity(claims, "auth_api");
            var principal = new ClaimsPrincipal(identity);
            AuthenticationState = new AuthenticationState(principal);
        }
        else
        {
            AuthenticationState = new AuthenticationState(new ClaimsPrincipal());
        }
        if (!ComparePrincipals(previousUser, AuthenticationState.User))
            NotifyAuthenticationStateChanged(Task.FromResult(AuthenticationState));
        return AuthenticationState;
    }

    private static bool ComparePrincipals(ClaimsPrincipal principal1, ClaimsPrincipal principal2)
    {
        if (principal1.Identity == null || principal2.Identity == null)
            return false;
        if (principal1.Identity.Name != principal2.Identity.Name)
            return false;
        if (principal1.Claims.Count() != principal2.Claims.Count())
            return false;
        foreach (var claim in principal1.Claims)
        {
            if (!principal2.HasClaim(claim.Type, claim.Value))
                return false;
        }
        return true;
    }

    private class User
    {
        public string Username { get; set; } = string.Empty;
        public List<Claim> Claims { get; set; } = [];
    }

    private class Claim
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
