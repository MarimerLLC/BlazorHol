# Authentication Fundamentals

## Creating the Solution

1. Open Visual Studio
2. Click on Create a new project
3. Select Blazor Web App
4. Click Next
5. Enter the project name: `BlazorHolAuthentication`
6. Click Next
7. Use the following options:
   - Framework: .NET 8.0
   - Authentication Type: None
   - Configure for HTTPS: Checked
   - Interactive render mode: Server
   - Interactivity location: Per page/component
   - Include sample pages: Checked
8. Click Create

For this lab, you will create a server-side Blazor application with per-page interactivity. This means that the application will run on the server and the interactivity will be managed on a per-page basis. This will provide the basis for understanding authentication in Blazor.

## Add Authentication Services

1. Open the `Program.cs` file in the project
2. Add the following service registrations:
    
```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie();
builder.Services.AddCascadingAuthenticationState();
```

This registers the cookie authentication service and the cascading authentication state service. The cascading authentication state service is used to provide the authentication state to all components in the application.

3. Add code to use authorization and authentication in the application

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

## Use Authentication in the Application

1. Edit the `_Imports.razor` file in the `Components` folder and add a `using` directive for the `Microsoft.AspNetCore.Components.Authorization` namespace:

```csharp
@using Microsoft.AspNetCore.Components.Authorization
```

2. Edit the `Routes.razor` file in the `Components` folder:

```html
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(Program).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
            <FocusOnNavigate RouteData="routeData" Selector="h1" />
        </Found>
    </Router>
</CascadingAuthenticationState>
```

This code sets up the authentication state for the application. The `CascadingAuthenticationState` component provides the authentication state to all components in the application. The `AuthorizeRouteView` component is used to display the correct content based on the user's authentication state.

## Add a Mock User Validation Service

1. Add a new folder named `Services` to the project
2. Add a new class to the `Services` folder named `UserValidation.cs`
3. Add the following code to the `UserValidation.cs` file:

```csharp
namespace BlazorHolAuthentication.Services;

public class UserValidation
{
    public bool ValidateUser(string username, string password)
    {
        if (username == "admin" && password == "admin")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public List<string> GetRoles(string username)
    {
        if (username == "admin")
        {
            return new List<string> { "Admin" };
        }
        else
        {
            return new List<string> { "User" };
        }
    }
}
```

This code creates a simple user validation service with a `ValidateUser` method that checks if the username and password are correct and a `GetRoles` method that returns the roles for a given username.

4. Register this service in the `Program.cs` file:

```csharp
builder.Services.AddTransient<UserValidation>();
```

## Add Login and Logout Pages

1. Add a new Razor component to the `Components/Pages` folder named `Login.razor`
2. Add the following code to the `Login.razor` file:

```html
@page "/login"

@using Microsoft.AspNetCore.Authentication
@using Microsoft.AspNetCore.Authentication.Cookies
@using System.Security.Claims

@inject Services.UserValidation UserValidation
@inject IHttpContextAccessor httpContextAccessor
@inject NavigationManager NavigationManager

<PageTitle>Login</PageTitle>

<h1>Login</h1>

<div>
  <EditForm Model="userInfo" OnSubmit="LoginUser" FormName="loginform">
      <div>
          <label>Username</label>
          <InputText @bind-Value="userInfo.Username" />
      </div>
      <div>
          <label>Password</label>
          <InputText type="password" @bind-Value="userInfo.Password" />
      </div>
      <button>Login</button>
  </EditForm>
</div>

<div style="background-color:lightgray">
  <p>User identities:</p>
  <p>admin, admin</p>
</div>

<div><p class="alert-danger">@Message</p></div>

@code {

    [SupplyParameterFromForm]
    public UserInfo userInfo { get; set; } = new();

    public string Message { get; set; } = "";

    private async Task LoginUser()
    {
        Message = "";
        ClaimsPrincipal principal;
        if (UserValidation.ValidateUser(userInfo.Username, userInfo.Password))
        {
            // create authenticated principal
            var identity = new ClaimsIdentity("custom");
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, userInfo.Username));
            var roles = UserValidation.GetRoles(userInfo.Username);
            foreach (var item in roles)
                claims.Add(new Claim(ClaimTypes.Role, item));
            identity.AddClaims(claims);
            principal = new ClaimsPrincipal(identity);

            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                Message = "HttpContext is null";
                return;
            }
            AuthenticationProperties authProperties = new AuthenticationProperties();
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            NavigationManager.NavigateTo("/");
        }
        else
        {
            Message = "Invalid credentials";
        }
    }


    public class UserInfo
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
```

There's a lot going on in this code. Here's a summary:

- The `Login` component is a form that allows the user to enter a username and password.
- The `LoginUser` method is called when the form is submitted. It validates the user's credentials and creates a `ClaimsPrincipal` object with the user's identity and roles.
- The `LoginUser` method then signs in the user using the `HttpContext.SignInAsync` method, which sets a cookie with the user's authentication information.
- If the user's credentials are invalid, an error message is displayed.

3. Add a new Razor component to the `Components/Pages` folder named `Logout.razor`
4. Add the following code to the `Logout.razor` file:

```html
@page "/logout"

@using Microsoft.AspNetCore.Authentication
@using Microsoft.AspNetCore.Authentication.Cookies

@inject IHttpContextAccessor httpContextAccessor
@inject NavigationManager NavigationManager

<h3>Logout</h3>

@code {
    protected override async Task OnInitializedAsync()
    {
        var httpContext = httpContextAccessor.HttpContext;
        var principal = httpContext.User;
        if (principal.Identity is not null && principal.Identity.IsAuthenticated)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        NavigationManager.NavigateTo("/");
    }
}
```

This component logs the user out by calling the `HttpContext.SignOutAsync` method, which removes the authentication cookie. It then redirects the user to the home page.

## Update the Navigation Menu

1. Edit the `MainLayout.razor` file in the `Components/Layout` folder

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
                    <a href="/logout">Logout</a>
                </Authorized>
                <NotAuthorized>
                    <a href="login">Login</a>
                </NotAuthorized>
            </AuthorizeView>
            <a href="https://learn.microsoft.com/aspnet/core/" target="_blank">About</a>
        </div>

        <article class="content px-4">
            @Body
        </article>
    </main>
</div>

<div id="blazor-error-ui">
    An unhandled error has occurred.
    <a href="" class="reload">Reload</a>
    <a class="dismiss">🗙</a>
</div>
```

This code adds a new `AuthorizeView` component to the layout. This component displays different content based on the user's authentication state. If the user is authenticated, it displays a welcome message with the user's name and a logout link. If the user is not authenticated, it displays a login link.

## Test the Application

1. Press F5 to run the application
2. Click on the `Login` link in the navigation menu
3. Enter `admin` for both the username and password
4. Click the `Login` button
5. You should see the home page with a welcome message and a logout link
6. Click the `Logout` link
7. You should be redirected to the home page with a login link
