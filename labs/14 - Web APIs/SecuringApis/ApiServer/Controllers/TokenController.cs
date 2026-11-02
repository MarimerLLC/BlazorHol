using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ApiServer.Controllers;

public record LoginRequest(string Username, string Password);

public record TokenResponse(string AccessToken, string TokenType, int ExpiresIn);

/// <summary>
/// Issues JWTs for the "Jwt" authentication scheme. The caller exchanges a
/// username and password for a signed token, then sends that token on each API call.
/// </summary>
[ApiController]
[Route("jwt/token")]
public class TokenController(IConfiguration configuration) : ControllerBase
{
    // Hardcoded demo users and the scopes each one is granted.
    // A real app would check credentials against a user store with hashed passwords.
    private static readonly Dictionary<string, (string Password, string[] Scopes)> Users = new()
    {
        ["alice"] = ("alice-password", ["weather.read"]),
        ["bob"] = ("bob-password", [])
    };

    [HttpPost]
    public ActionResult<TokenResponse> CreateToken(LoginRequest request)
    {
        if (!Users.TryGetValue(request.Username, out var user) || user.Password != request.Password)
        {
            return Unauthorized();
        }

        var settings = configuration.GetSection("Jwt");
        var lifetime = TimeSpan.FromMinutes(30);

        var claims = new List<Claim>
        {
            new("sub", request.Username),
            new("name", request.Username)
        };
        if (user.Scopes.Length > 0)
        {
            claims.Add(new Claim("scope", string.Join(' ', user.Scopes)));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings["Issuer"],
            Audience = settings["Audience"],
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(lifetime),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings["SigningKey"]!)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new TokenResponse(token, "Bearer", (int)lifetime.TotalSeconds);
    }
}
