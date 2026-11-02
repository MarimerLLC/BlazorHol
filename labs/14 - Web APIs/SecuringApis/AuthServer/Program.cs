using System.Security.Claims;
using AuthServer;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

// OpenIddict stores its clients (applications) and scopes in a database.
// An in-memory database keeps this demo self-contained.
builder.Services.AddDbContext<DbContext>(options =>
{
    options.UseInMemoryDatabase("AuthServer");
    options.UseOpenIddict();
});

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore().UseDbContext<DbContext>();
    })
    .AddServer(options =>
    {
        // The OAuth 2.0 token endpoint: clients POST here to get an access token
        options.SetTokenEndpointUris("connect/token");

        // Machine-to-machine flow: a client authenticates with its own id and secret
        options.AllowClientCredentialsFlow();

        options.RegisterScopes("weather.read", "alerts.read");

        // Demo keys, regenerated each time the server starts. A real server
        // uses persistent certificates. The public signing key is published
        // at /.well-known/jwks so APIs can validate the tokens.
        options.AddEphemeralEncryptionKey()
               .AddEphemeralSigningKey();

        // OpenIddict encrypts access tokens by default. Turning that off produces
        // a standard signed JWT that any API can validate with the JwtBearer handler.
        options.DisableAccessTokenEncryption();

        // Let our own code (below) build the token's claims
        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough();
    });

builder.Services.AddHostedService<SeedData>();

var app = builder.Build();

app.UseHttpsRedirection();

// The token endpoint. OpenIddict has already validated the client id, secret,
// grant type, and requested scopes before this code runs.
app.MapPost("/connect/token", async (
    HttpContext context,
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictScopeManager scopeManager) =>
{
    var request = context.GetOpenIddictServerRequest()
        ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

    if (!request.IsClientCredentialsGrantType())
    {
        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    var application = await applicationManager.FindByClientIdAsync(request.ClientId!)
        ?? throw new InvalidOperationException("The application cannot be found.");

    var identity = new ClaimsIdentity(
        TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

    // The caller is the client application itself
    identity.SetClaim(Claims.Subject, await applicationManager.GetClientIdAsync(application));
    identity.SetClaim(Claims.Name, await applicationManager.GetDisplayNameAsync(application));

    // The granted scopes become the token's "scope" claim, and the APIs (resources)
    // associated with those scopes become the token's "aud" (audience) claim
    identity.SetScopes(request.GetScopes());
    var resources = new List<string>();
    await foreach (var resource in scopeManager.ListResourcesAsync(identity.GetScopes()))
    {
        resources.Add(resource);
    }
    identity.SetResources(resources);

    // Put every claim in the access token
    identity.SetDestinations(static claim => [Destinations.AccessToken]);

    return Results.SignIn(new ClaimsPrincipal(identity),
        authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
});

app.Run();
