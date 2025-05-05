using BlazorStyles.Data;
using System.Security.Claims;

namespace BlazorStyles.Services;

public class ValidateUser
{
    public Task<UserInfo> Validate(string username, string password)
    {
        UserInfo? user = null;
        if (username == "admin" && password == "admin")
        {
            var claims = new List<ClaimInfo>
            {
                new(ClaimTypes.Name, username),
                new(ClaimTypes.Role, "Admin")
            };
            user = new UserInfo
            {
                Name = username,
                Claims = claims.Select(c => new ClaimInfo { Type = c.Type, Value = c.Value }).ToList()
            };
        }
        else
        {
            user = new UserInfo();
        }
        return Task.FromResult(user);
    }
}
