using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlazorHolWasmAuthentication.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IHttpContextAccessor httpContextAccessor)
{
    [HttpGet]
    public User GetUser()
    {
        ClaimsPrincipal principal = httpContextAccessor!.HttpContext!.User;
        if (principal != null && principal.Identity != null && principal.Identity.IsAuthenticated)
        {
            // Return a user object with the username and claims
            var claims = principal.Claims.Select(c => new Claim { Type = c.Type, Value = c.Value }).ToList();
            return new User
            {
                Username = principal.Identity!.Name,
                Claims = claims
            };
        }
        else
        {
            // Return an empty user object
            return new User();
        }
    }
}

public class Credentials
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class User
{
    public string Username { get; set; } = string.Empty;
    public List<Claim> Claims { get; set; } = [];
}

public class Claim
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}