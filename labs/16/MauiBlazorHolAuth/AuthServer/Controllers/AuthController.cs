using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthServer.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController
{
    [HttpPost]
    public User Authenticate(Credentials credentials)
    {
        // Check if the credentials are valid
        if (credentials.Username == "admin" && credentials.Password == "admin")
        {
            // Return the user object
            return new User
            {
                Username = "admin",
                Claims = [
                    new Claim { Type = ClaimTypes.Name, Value = "admin" },
                    new Claim { Type = ClaimTypes.Role, Value = "admin" },
                    new Claim { Type = "auth-token", Value = "MyAuthToken" }
                ]
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

public class  User
{
    public string Username { get; set; } = string.Empty;
    public List<Claim> Claims { get; set; } = [];
}

public class Claim
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
