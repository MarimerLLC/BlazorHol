using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

namespace BlazorHolAuthentication.Services
{
    public class CustomAuthenticationStateProvider : ServerAuthenticationStateProvider
    {
        private readonly ClaimsPrincipal anonymous = new(new ClaimsIdentity());

        public CustomAuthenticationStateProvider()
        {
            SetAuthenticationState(Task.FromResult(
                new AuthenticationState(anonymous)));
        }

        public void Logout()
        {
            var authenticationState = Task.FromResult(
                new AuthenticationState(anonymous));
            SetAuthenticationState(authenticationState);
            NotifyAuthenticationStateChanged(authenticationState);
        }
    }
}
