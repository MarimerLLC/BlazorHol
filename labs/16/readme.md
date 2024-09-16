# Authentication in MAUI Blazor Hybrid Apps

## Creating the Solution

1. Open Visual Studio
2. Click on Create a new project
3. Select .NET MAUI Blazor Hybrid App
4. Click Next
5. Enter the project name: `MauiBlazorHolAuth`
6. Click Next
7. Use the following options:
   - Target Framework: .NET 8.0
8. Click Create

## Adding an API Server project

1. Right-click on the solution
2. Click on Add > New Project
3. Select ASP.NET Core Web API
4. Click Next
5. Enter the project name: `AuthServer`
6. Click Next
7. Use the following options:
   - Target Framework: .NET 8.0
   - Authentication Type: None
   - Configure for HTTPS: Checked
   - Use controllers: Checked
8. Click Create

## Creating an Authentication Controller

1. Add a new controller to the `AuthServer` project named `AuthController`
2. Add the following code to the `AuthController` class:

```csharp
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
```

In a real world scenario, you would use a database or LDAP server to validate the credentials and return the user claims.

## Adding Authentication to the MAUI Blazor App

1. Open the `MauiBlazorHolAuth` project
2. Add a reference to the `Microsoft.AspNetCore.Components.Authorization` NuGet package
3. Add a `CustomAuthenticationStateProvider` class to the project:

```csharp
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace MauiBlazorHolAuth;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private AuthenticationState AuthenticationState { get; set; } =
        new AuthenticationState(new ClaimsPrincipal());

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(AuthenticationState);
    }

    public void SetPrincipal(ClaimsPrincipal principal)
    {
        AuthenticationState = new AuthenticationState(principal);
        NotifyAuthenticationStateChanged(Task.FromResult(AuthenticationState));
    }
}
```

4. Add the following code to the `MauiProgram.cs` file:

```csharp
   builder.Services.AddAuthorizationCore();
   builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
```

5. Add a `@using` statement to `_Imports.razor`:

```csharp
@using Microsoft.AspNetCore.Components.Authorization
```

6. Add the `CascadeAuthenticationState` component and `AuthorizeRouteView` to the `Routes.razor` file:

```html
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(MauiProgram).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(Layout.MainLayout)" />
            <FocusOnNavigate RouteData="@routeData" Selector="h1" />
        </Found>
    </Router>
</CascadingAuthenticationState>
```

7. Add a `Login` page to the `Pages` folder:

```html
@page "/login"

@using System.ComponentModel.DataAnnotations
@using System.Security.Claims

@inject AuthenticationStateProvider AuthenticationStateProvider
@inject NavigationManager NavigationManager

<h3>Login</h3>

<p class="alert-danger">@ErrorMessage</p>

<EditForm Model="@credentials" OnValidSubmit="HandleValidSubmit">
    <DataAnnotationsValidator />
    <ValidationSummary />
    <div class="form-group">
        <label for="username">Username</label>
        <InputText id="username" class="form-control" @bind-Value="credentials.Username" />
        <ValidationMessage For="@(() => credentials.Username)" />
    </div>
    <div class="form-group">
        <label for="password">Password</label>
        <InputText id="password" class="form-control" @bind-Value="credentials.Password" />
        <ValidationMessage For="@(() => credentials.Password)" />
    </div>
    <button type="submit" class="btn btn-primary">Login</button>
</EditForm>


@code
{
    private Credentials credentials = new Credentials();
    private string? ErrorMessage { get; set; }

    private async Task HandleValidSubmit()
    {
        var httpClient = new HttpClient();
        var response = await httpClient.PostAsJsonAsync("https://localhost:7003/auth", credentials);
        var user = await response.Content.ReadFromJsonAsync<User>();
        if (user != null && !string.IsNullOrEmpty(user.Username))
        {
            var claims = new List<System.Security.Claims.Claim>();
            foreach (var claim in user.Claims)
            {
                claims.Add(new System.Security.Claims.Claim(claim.Type, claim.Value));
            }
            var identity = new ClaimsIdentity(claims, "auth_api");
            var principal = new ClaimsPrincipal(identity);
            ((CustomAuthenticationStateProvider)AuthenticationStateProvider).SetPrincipal(principal);
            NavigationManager.NavigateTo("/");
        }
        else
        {
            ErrorMessage = "Invalid credentials";
        }
    }

    private class Credentials
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    private class User
    {
        public string Username { get; set; } = string.Empty;
        public List<Claim> Claims { get; set; } = [];
    }

    private class Claim
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
```

8. Add a `Logout` page to the `Pages` folder:

```html
@page "/logout"

@using System.Security.Claims

@inject AuthenticationStateProvider AuthenticationStateProvider
@inject NavigationManager NavigationManager

@code {
    protected override void OnInitialized()
    {
        ((CustomAuthenticationStateProvider)AuthenticationStateProvider).SetPrincipal(
            new ClaimsPrincipal());
        NavigationManager.NavigateTo("/");
    }
}
```

9. Edit the `MainLayout.razor` file to add a login/logout button:

```html
@inherits LayoutComponentBase

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <div class="top-row px-4">
            <AuthorizeView>
                <Authorized>
                    Hello, @context.User.Identity.Name
                    <a href="logout">Logout</a>
                </Authorized>
                <NotAuthorized>
                    <a href="login">Login</a>
                </NotAuthorized>
            </AuthorizeView>
        </div>

        <article class="content px-4">
            @Body
        </article>
    </main>
</div>
```

## Running the Solution

1. Right-click on the solution in Solution Explorer
2. Click on Set Startup Projects
3. Select Multiple startup projects
4. Set the Action for the `AuthServer` project to Start
5. Set the Action for the `MauiBlazorHolAuth` project to Start
6. Click OK
7. Press F5 to run the solution
8. The MAUI Blazor app should open in the browser
9. Click on the Login link in the top right corner
10. Enter the username `admin` and password `admin`
11. Click on the Login button
12. You should see the Home page with the message `Hello, admin`
13. Click on the Logout link in the top right corner
14. You should be redirected to the Login page

## Running the App on Android

The Android emulator can't directly access localhost, so you need some extra software to make it work. You can use [ngrok](https://ngrok.com/) to create a secure tunnel to your local server.

This is outside the scope of this lab, but you can follow the instructions on the ngrok website to set it up.
