# State Management

## Creating the Solution

1. Open Visual Studio
2. Click on Create a new project
3. Select Blazor Web App
4. Click Next
5. Enter the project name: `BlazorHolState`
6. Click Next
7. Use the following options:
   - Framework: .NET 8.0
   - Authentication Type: None
   - Configure for HTTPS: Checked
   - Interactive render mode: Auto (Server and WebAssembly)
   - Interactivity location: Per page/component
   - Include sample pages: Checked
8. Click Create

## Add Render Detection Services

1. Add the package reference

In the server project:

```text
dotnet package add Marimer.Blazor.RenderMode
```

In the client project:

```text
dotnet package add Marimer.Blazor.RenderMode.WebAssembly
```

2. Register the services 

In the server `Program.cs` file:

```csharp
using Marimer.Blazor.RenderMode;

builder.Services.AddRenderModeDetection();
```

In the client `Program.cs` file:

```csharp
using Marimer.Blazor.RenderMode.WebAssembly;

builder.Services.AddRenderModeDetection();
```

This makes the render mode detection services available to the application. These are the same services you implemented in a previous lab, but now they are available as a package.

## Define Session and Interfaces

1. In the _client_ project add a `Session` class:

```csharp
namespace BlazorHolState;

/// <summary>
/// Per-user session data. The object must be 
/// serializable via JSON.
/// </summary>
public class Session : Dictionary<string, string>
{
    /// <summary>
    /// Gets or sets the Session Id value.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;
    public bool IsCheckedOut { get; set; }
}
```

This type maintains the session data for the user. It is a dictionary of string values that can be serialized to JSON. The `SessionId` property is a unique identifier for the session. The `IsCheckedOut` property is a flag that indicates whether the user has checked out the dictionary to the WebAssembly client.

2. In the _client_ project add an `ISessionManager` interface:

```csharp
namespace BlazorHolState;

public interface ISessionManager
{
    Task<Session> GetSession();
    Task UpdateSession(Session session);
}
```

This interface defines the methods that the session service must implement. The `Session` type is the session data object.

## Create the Server Implementation

1. Add a new folder to the _server_ project called `Services`
1. In the `Services` folder, add a new class called `SessionIdManager`:

```csharp
namespace BlazorHolState.Server;

public class SessionIdManager(IHttpContextAccessor httpContextAccessor)
{
    private readonly IHttpContextAccessor HttpContextAccessor = httpContextAccessor;

    public Task<string?> GetSessionId()
    {
        var httpContext = HttpContextAccessor.HttpContext;
        string? result;

        if (httpContext != null)
        {
            if (httpContext.Request.Cookies.ContainsKey("sessionId"))
            {
                result = httpContext.Request.Cookies["sessionId"];
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
```

This service is responsible for managing the session id for the user. It uses the `IHttpContextAccessor` to access the current HTTP context. If the session id is not available in the request cookies, it generates a new one and adds it to the response cookies.

2. In the `Services` folder, add a new class called `SessionManager`:

```csharp
using System.Threading;

namespace BlazorHolState.Server;

/// <summary>
/// Dictionary containing per-user session objects, keyed
/// by sessionId.
/// </summary>
public class SessionManager : ISessionManager
{ 
    private Dictionary<string, Session> _sessions = new Dictionary<string, Session>();
    private readonly ISessionIdManager _sessionIdManager;

    public SessionManager(ISessionIdManager sessionIdManager)
    {
        _sessionIdManager = sessionIdManager;
    }

    public async Task<Session> GetSession()
    {
        var key = await _sessionIdManager.GetSessionId();
        if (!_sessions.ContainsKey(key))
            _sessions.Add(key, new Session());
        var session = _sessions[key];
        var endTime = DateTime.Now + TimeSpan.FromSeconds(10);
        while (session.IsCheckedOut)
        {
            if (DateTime.Now > endTime)
                throw new TimeoutException();
            await Task.Delay(5);
        }

        return session;
    }

    public async Task UpdateSession(Session session)
    {
        if (session != null)
        {
            var key = await _sessionIdManager.GetSessionId();
            session.SessionId = key;
            Replace(session, _sessions[key]);
            _sessions[key].IsCheckedOut = false;
        }
    }

    /// <summary>
    /// Replace the contents of oldSession with the items
    /// in newSession.
    /// </summary>
    /// <param name="newSession"></param>
    /// <param name="oldSession"></param>
    private void Replace(Session newSession, Session oldSession)
    {
        oldSession.Clear();
        foreach (var key in newSession.Keys)
            oldSession.Add(key, newSession[key]);
    }
}
```

This service manages the session data for the user. It uses the `ISessionIdManager` to get the session id and access the session data dictionary. The `GetSession` method retrieves the session data for the user, creating a new session if one does not exist. The `UpdateSession` method updates the session data with the new values.

3. Register the services in the _server_ `Program.cs` file:

```csharp
```

## Create the Client Implementation

1. Add a new folder to the _client_ project called `Services`
1. In the `Services` folder, add a new class called `SessionManager`:

```csharp
```

This service is responsible for managing the session data for the user. It uses the `HttpClient` to communicate with the server to get and update the session data.

2. Register the services in the _client_ `Program.cs` file:

```csharp
```
