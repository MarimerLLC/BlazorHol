# Render Modes

## Creating the Solution

1. Open Visual Studio
2. Click on Create a new project
3. Select Blazor Web App
4. Click Next
5. Enter the project name: `BlazorRenderModes`
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

1. Add a new folder to _both projects_ called `Services`
1. In the _client_ project `Services` folder, add a new class called `RenderMode`:

```csharp
namespace BlazorRenderModes.Services;

public enum RenderMode
{
    Unknown,
    ServerStatic,
    ServerStaticStreaming,
    ServerInteractive,
    WebAssemblyInteractive
}

public static class RenderModeFlagsExtensions
{
    public static bool IsServer(this RenderMode mode)
    {
        return mode.ToString().Contains("Server");
    }

    public static bool IsWebAssembly(this RenderMode mode)
    {
        return mode == RenderMode.WebAssemblyInteractive;
    }

    public static bool IsInteractive(this RenderMode mode)
    {
        return mode.ToString().Contains("Interactive");
    }

    public static bool IsStreaming(this RenderMode mode)
    {
        return mode == RenderMode.ServerStaticStreaming;
    }
}
```

These types are the list of render modes available in a Blazor app. They will be used by the rest of the types in the solution to determine the current render mode.

3. In the _client_ project `Services` folder, add a new class called `ActiveCircuitState`:

```csharp
namespace BlazorRenderModes.Services;

public class ActiveCircuitState
{
    /// <summary>
    /// Gets or sets a value indicating whether a SignalR
    /// circuit exists.
    /// </summary>
    public bool CircuitExists { get; set; }
}
```

This type will be used to track the state of the current SignalR circuit (if one exists).

4. In the _server_ project `Services` folder, add a new class called `ActiveCircuitHandler`:

```csharp
namespace BlazorRenderModes.Services;

using Microsoft.AspNetCore.Components.Server.Circuits;

public class ActiveCircuitHandler(ActiveCircuitState state) : CircuitHandler
{
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        state.CircuitExists = true;
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        state.CircuitExists = false;
        return base.OnCircuitClosedAsync(circuit, cancellationToken);
    }
}
```

This service detects when a SignalR circuit is opened or closed and updates the `ActiveCircuitState` accordingly.

Notice that this only exists on the _server_ project. In the client project there is never a SignalR circuit, and so this service is not needed.

5. In the _client_ project `Services` folder, add a new class called `RenderModeProvider`:

```csharp
using Microsoft.AspNetCore.Components;

namespace BlazorRenderModes.Services;

public class RenderModeProvider(ActiveCircuitState activeCircuitState)
{
    public RenderMode GetRenderMode(ComponentBase page)
    {
        RenderMode result = RenderMode.ServerStatic;
        var isBrowser = OperatingSystem.IsBrowser();
        if (isBrowser)
            result = RenderMode.WebAssemblyInteractive;
        else if (activeCircuitState.CircuitExists)
            result = RenderMode.ServerInteractive;
        else if (page.GetType().GetCustomAttributes(typeof(StreamRenderingAttribute), true).Length > 0)
            result = RenderMode.ServerStaticStreaming;
        return result;
    }
}
```

This is the class that will determine the current render mode based on the current environment and the state of the SignalR circuit.

6. In the _server_ project open the `Program.cs` file and add the following code to register services:
    
```csharp
builder.Services.AddTransient<RenderModeProvider>();
builder.Services.AddScoped<ActiveCircuitState>();
builder.Services.AddScoped(typeof(CircuitHandler), typeof(ActiveCircuitHandler));
```

This registers the services that will be used to detect the current render mode.

7. In the _client_ project open the `Program.cs` file and add the following code to register services:

```csharp
builder.Services.AddTransient<RenderModeProvider>();
builder.Services.AddScoped<ActiveCircuitState>();
```

This registers the services that will be used to detect the current render mode.

## Using Render Mode Detection

1. Open the 'Home.razor' file in the server project.
2. Add the following code to the file:

```csharp
@page "/"

@inject Services.RenderModeProvider RenderModeProvider

<PageTitle>Home</PageTitle>

<h1>Hello, world!</h1>

Welcome to your new app.

<p class="border border-info">Render mode: @RenderModeProvider.GetRenderMode(this)</p>
```

3. Run the application
4. Notice that the render mode is displayed on the home page, and it is ServerStatic.
5. Open the 'Counter.razor' file in the client project.
6. Inject the `RenderModeProvider` service into the file and use it in a `<p>` tag to display the render mode.
5. Open the 'Weather.razor' file in the server project.
6. Inject the `RenderModeProvider` service into the file and use it in a `<p>` tag to display the render mode.
7. Run the application
8. Notice that the render mode is displayed on the Counter and Weather pages as well.

Watch closely as you navigate between pages. The render mode will change as you navigate between pages, and _within_ the `Counter` page as it moves from static to interactive and server to WebAssembly.

## Using Render Mode Detection in Components

1. Open the `Counter.razor` file.
2. Add the following code to the file:

```html
@page "/counter"
@rendermode InteractiveAuto

@using BlazorRenderModes.Services

@inject Services.RenderModeProvider RenderModeProvider

<PageTitle>Counter</PageTitle>

<h1>Counter</h1>

<p class="border border-info">Render mode: @RenderModeProvider.GetRenderMode(this)</p>

<p role="status">Current count: @currentCount</p>

@if (RenderModeProvider.GetRenderMode(this).IsInteractive())
{
    <button class="btn btn-primary" @onclick="IncrementCount">Click me</button>
}

@code {
    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }
}
```

3. Run the application
4. Navigate to the `Counter` page
5. Notice that the button only appears once the page has become interactive
6. Navigate away from the `Counter` page and back again
7. Notice that the button only appears once the page has become interactive
