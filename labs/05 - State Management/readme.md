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
    public string SessionId { get => this["__sessionId"]; set => this["__sessionId"] = value; }
}
```

This type maintains the session data for the user. It is a dictionary of string values that can be serialized to JSON. The `SessionId` property is a unique identifier for the session. 

2. In the _client_ project add an `ISessionManager` interface:

```csharp
namespace BlazorHolState;

public interface ISessionManager
{
    Task<Session> GetSessionAsync();
    Task UpdateSessionAsync(Session session);
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
```

This service is responsible for managing the session id for the user. It uses the `IHttpContextAccessor` to access the current HTTP context. If the session id is not available in the request cookies, it generates a new one and adds it to the response cookies.

2. In the `Services` folder, add a new class called `SessionManager`:

```csharp
namespace BlazorHolState.Server;

/// <summary>
/// Dictionary containing per-user session objects, keyed
/// by sessionId.
/// </summary>
public class SessionManager(SessionIdManager sessionIdManager) : ISessionManager
{ 
    private readonly Dictionary<string, Session> _sessions = [];

    public async Task<Session> GetSessionAsync()
    {
        var key = await sessionIdManager.GetSessionIdAsync();
        if (!_sessions.ContainsKey(key))
            _sessions.Add(key, []);
        var session = _sessions[key];
        session.SessionId = key;
        return session;
    }

    public async Task UpdateSessionAsync(Session session)
    {
        if (session != null)
        {
            var key = await sessionIdManager.GetSessionIdAsync();
            session.SessionId = key;
            Replace(session, _sessions[key]);
        }
    }

    /// <summary>
    /// Replace the contents of oldSession with the items
    /// in newSession.
    /// </summary>
    /// <param name="newSession"></param>
    /// <param name="oldSession"></param>
    private static void Replace(Session newSession, Session oldSession)
    {
        if (ReferenceEquals(newSession, oldSession))
            return;
        oldSession.Clear();
        foreach (var key in newSession.Keys)
            oldSession.Add(key, newSession[key]);
    }
}
```

This service manages the session data for the user. It uses the `ISessionIdManager` to get the session id and access the session data dictionary. The `GetSession` method retrieves the session data for the user, creating a new session if one does not exist. The `UpdateSession` method updates the session data with the new values.

3. Register the services in the _server_ `Program.cs` file:

```csharp
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton(typeof(ISessionManager), typeof(SessionManager));
builder.Services.AddTransient(typeof(SessionIdManager), typeof(SessionIdManager));
```

The `ISessionManager` service is a singleton, so there is one instance for the entire server. This stores all the session state for all users on the server.

The `SessionIdManager` is transient, meaning that an instance is created whenever one is needed. This way it will return per-user values.

## Create the Client Implementation

1. Add a new folder to the _client_ project called `Services`
1. In the `Services` folder, add a new class called `SessionManager`:

```csharp
using System.Net.Http.Json;

namespace BlazorHolState.Client;

/// <summary>
/// Session objects for the current user
/// </summary>
public class SessionManager(HttpClient client) : ISessionManager
{
    private Session? _session;

    public async Task<Session> GetSessionAsync()
    {
        _session = await client.GetFromJsonAsync<Session>("state");
        if (_session == null)
            throw new InvalidOperationException("Session not found");
        return _session;
    }

    public async Task UpdateSessionAsync(Session session)
    {
        await client.PutAsJsonAsync<Session>("state", session);
        _session = session;
    }
}
```

This service is responsible for managing the session data for the user. It uses the `HttpClient` to communicate with the server to get and update the session data.

2. Register the services in the _client_ `Program.cs` file:

```csharp
builder.Services.AddTransient<HttpClient>(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton(typeof(ISessionManager), typeof(SessionManager));
```

In the client WebAssembly environment there is only ever the one user of the browser, so this `ISessionManager` service is a singleton so it is available for the current user.

## Add the Server Controller

In the server project, add a `Controllers` directory to the project.

In the `Controllers` directory, add a `StateController.cs` file:

```csharp
using BlazorHolState;
using Microsoft.AspNetCore.Mvc;

namespace BlazorHolState.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StateController : ControllerBase
    {
        private readonly ILogger<StateController> _logger;
        private readonly ISessionManager _sessionList;

        public StateController(ISessionManager sessionList, ILogger<StateController> logger)
        {
            _logger = logger;
            _sessionList = sessionList;
        }

        [HttpGet(Name = "GetState")]
        public async Task<Session> Get()
        {
            var session = await _sessionList.GetSessionAsync();
            return session;
        }

        [HttpPut(Name = "UpdateState")]
        public async Task Put(Session updatedSession)
        {
            await _sessionList.UpdateSessionAsync(updatedSession);
        }
    }
}
```

In the `Program.cs` file of the web server project, add the code to enable controllers.

First, when registering services near the top of the file:

```csharp
builder.Services.AddControllers();
```

Second, right before the call to `MapRazorComponents` add this:

```csharp
app.MapControllers();
```

This controller is used by the WebAssembly implementation of `ISessionManager` to get and set the current user's session dictionary on the server.

## Using the State Manager

Now that the app has a basic state management implementation, the next step is to use it in the various components of the app.

### Home Page

Open the `Home.razor` file in the server project and inject a couple of services:

```csharp
@inject Marimer.Blazor.RenderMode.RenderModeProvider RenderModeProvider
@inject ISessionManager SessionManager
```

In the code block, use the services to initialize some fields with information about the render mode, session id, and a session value:

```csharp
    private string? renderMode;
    private string? sessionId;
    private string? sessionValue;

    protected override async Task OnInitializedAsync()
    {
        renderMode = RenderModeProvider.GetRenderMode(this).ToString();
        var session = await SessionManager.GetSessionAsync();
        sessionId = session.SessionId;
        if (!session.ContainsKey("test"))
            session["test"] = Guid.NewGuid().ToString();
        sessionValue = session["test"] as string;
    }
```

In the markup part of the page, display the values:

```html
<p class="alert-info">Render mode: @renderMode</p>
<p class="alert-warning">Session id: @sessionId</p>
<p class="alert-primary">Session value: @sessionValue</p>
```

### Weather Page

Repeat in the `Weather.razor` file.

Inject the services:

```csharp
@inject Marimer.Blazor.RenderMode.RenderModeProvider RenderModeProvider
@inject ISessionManager SessionManager
```

Define and set the fields in the code block:

```csharp
    private string? renderMode;
    private string? sessionId;
    private string? sessionValue;
    private WeatherForecast[]? forecasts;

    protected override async Task OnInitializedAsync()
    {
        renderMode = RenderModeProvider.GetRenderMode(this).ToString();
        var session = await SessionManager.GetSessionAsync();
        sessionId = session.SessionId;
        sessionValue = session["test"] as string;

        // Simulate asynchronous loading to demonstrate streaming rendering
        await Task.Delay(500);
        ...
```

Display the field values in the markup:

```html
<p class="alert-info">Render mode: @renderMode</p>
<p class="alert-warning">Session id: @sessionId</p>
<p class="alert-primary">Session value: @sessionValue</p>
```

### Counter Page

The `Counter.razor` file is in the client project, and this one will require a bit more work, because you can use session state to maintain the current count value from this page.

Normally, any time you leave and return to the Counter page, the current value resets to 0. Using the session state concept, this value can be maintained in memory by the app.

First, inject the services into the component:

```csharp
@using Marimer.Blazor.RenderMode

@inject RenderModeProvider RenderModeProvider
@inject ISessionManager SessionManager
```

Then define and initialize the same fields as in the other components:

```csharp
    private Marimer.Blazor.RenderMode.RenderMode renderMode;
    private string? sessionId;
    private string? sessionValue;
    private int currentCount = 0;
    Session? session;

    protected override async Task OnInitializedAsync()
    {
        renderMode = RenderModeProvider.GetRenderMode(this);
        session = await SessionManager.GetSessionAsync();
        sessionId = session.SessionId;
        ...
```

And display those fields in the markup:

```html
<p class="alert-info">Render mode: @renderMode</p>
<p class="alert-warning">Session id: @sessionId</p>
<p class="alert-primary">Session value: @sessionValue</p>
```

With that done, you can now add code to maintain the `currentCount` value in session state.

In the `OnInitializedAsync` method, as the component is loaded, get the value (if any) from session state:

```csharp
    protected override async Task OnInitializedAsync()
    {
        renderMode = RenderModeProvider.GetRenderMode(this);
        session = await SessionManager.GetSessionAsync();
        sessionId = session.SessionId;
        sessionValue = session["test"] as string;
        if (!session.ContainsKey("count"))
        {
            session["count"] = "0";
        }
        currentCount = int.Parse(session["count"]);
    }
```

Then in the `IncrementCount` method, store the incremented value into session state:

```csharp
    private async Task IncrementCount()
    {
        currentCount++;
        session!["count"] = currentCount.ToString();
        await SessionManager.UpdateSessionAsync(session);
    }
```

Because this method mutates the session state dictionary, the `UpdateSessionAsync` method is called to ensure those changes are saved on the server. This only does work if the code is running in WebAssembly. On the server it does nothing.

In WebAssembly though, it is necessary to send the copy of the mutated state data to the server so the server stays in sync with the client. This way, if the user navigates to some server static or server interactive page, any state changed on the WebAssembly client will be consistently available to all server-side code as well.

## Run the App

Press F5 or ctrl-F5 to run the app.

You should see values on the home page indicating the session id and a unique session value - both guids.

Navigate to the weather page, and the values should be the same.

Navigate to the counter page. Notice that the values are the same, and the page should be in server interactive mode.

Click the button to increment the count.

Then click to the home page to switch into server static render mode (releasing the SignalR connection), and click back to the counter page.

Now the counter page should be in WebAssembly interactive mode, and the values should still be consistent - including the current counter value.
