using System.Text;
using ApiServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var oauthSettings = builder.Configuration.GetSection("OAuth");

// Two authentication schemes, both of which validate a JWT bearer token.
// The difference is who issues the token and how the API gets the key to check it.
builder.Services.AddAuthentication()
    // "Jwt": this API issues its own tokens (see TokenController) and validates
    // them with a secret signing key that only this API knows.
    .AddJwtBearer(AuthSchemes.Jwt, options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SigningKey"]!)),
            NameClaimType = "name"
        };
    })
    // "OAuth": tokens are issued by a separate OAuth 2.0 authorization server
    // (the AuthServer project). The API downloads the server's discovery document
    // and public signing keys from the Authority URL, so no secret is shared.
    .AddJwtBearer(AuthSchemes.OAuth, options =>
    {
        options.Authority = oauthSettings["Authority"];
        options.Audience = oauthSettings["Audience"];
        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = "name";
    });

// Each policy says which scheme authenticates the caller, and which scope
// the caller's token must contain.
builder.Services.AddSingleton<IAuthorizationHandler, ScopeAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.JwtWeatherRead, policy =>
    {
        policy.AddAuthenticationSchemes(AuthSchemes.Jwt);
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ScopeRequirement("weather.read"));
    });
    options.AddPolicy(Policies.OAuthWeatherRead, policy =>
    {
        policy.AddAuthenticationSchemes(AuthSchemes.OAuth);
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ScopeRequirement("weather.read"));
    });
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
