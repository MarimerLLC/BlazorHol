using Microsoft.AspNetCore.Mvc;
using BlazorStyles.Data;

namespace BlazorStyles.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IHttpContextAccessor httpContextAccessor) : ControllerBase
{
    [HttpGet]
    public Task<UserInfo> Get()
    {
        var httpContext = httpContextAccessor.HttpContext;
        var principal = httpContext!.User;
        if (principal == null)
        {
            return Task.FromResult(new UserInfo());
        }
        else
        {
            var user = new UserInfo
            {
                Name = principal.Identity!.Name ?? "",
                Claims = principal.Claims.Select(c => new ClaimInfo { Type = c.Type, Value = c.Value }).ToList()
            };
            return Task.FromResult(user);
        }
    }
}
