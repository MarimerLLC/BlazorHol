using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthServer;

/// <summary>
/// Registers the demo scopes and client applications when the server starts.
/// </summary>
public class SeedData(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // Scopes, and the API (resource) each one grants access to.
        // Tokens with either scope have "weather-api" as their audience.
        await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
        {
            Name = "weather.read",
            Resources = { "weather-api" }
        }, cancellationToken);

        await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
        {
            Name = "alerts.read",
            Resources = { "weather-api" }
        }, cancellationToken);

        // A client that is allowed to request the weather.read scope
        await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "weather-client",
            ClientSecret = "weather-client-secret",
            ClientType = ClientTypes.Confidential,
            DisplayName = "Weather client",
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.ClientCredentials,
                Permissions.Prefixes.Scope + "weather.read"
            }
        }, cancellationToken);

        // A client that can call the API, but is only allowed the alerts.read scope
        await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "alerts-client",
            ClientSecret = "alerts-client-secret",
            ClientType = ClientTypes.Confidential,
            DisplayName = "Alerts client",
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.ClientCredentials,
                Permissions.Prefixes.Scope + "alerts.read"
            }
        }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
