using BlazorStyles.Data;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace BlazorStyles.Client.Services;

public class RemoteAuthenticationStateProvider(HttpClient httpClient) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = await httpClient.GetFromJsonAsync<UserInfo>("api/auth");
        if (user == null)
        {
            return new AuthenticationState(new ClaimsPrincipal());
        }
        else
        {
            var identity = user.IsAuthenticated ?
                new ClaimsIdentity(user.Claims.Select(c => new Claim(c.Type, c.Value)), "serverauth") : new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationState(principal);
        }
    }
}
