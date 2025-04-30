namespace BlazorHolState.Server;

public class SessionIdManager(IHttpContextAccessor httpContextAccessor)
{
    private readonly IHttpContextAccessor HttpContextAccessor = httpContextAccessor;

    public Task<string> GetSessionIdAsync()
    {
        var httpContext = HttpContextAccessor.HttpContext;
        string result;

        if (httpContext != null)
        {
            if (httpContext.Request.Cookies.ContainsKey("sessionId"))
            {
                result = httpContext.Request.Cookies["sessionId"]!;
            }
            else
            {
                result = Guid.NewGuid().ToString();
                httpContext.Response.Cookies.Append("sessionId", result);
            }
        }
        else
        {
            throw new InvalidOperationException("No HttpContext available");
        }
        return Task.FromResult(result);
    }
}
