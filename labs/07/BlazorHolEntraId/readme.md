# Authentication via Oauth (Entra Id)

## Creating the Solution

1. Open Visual Studio
2. Click on Create a new project
3. Select Blazor WebAssembly Standalone App
4. Click Next
5. Enter the project name: `BlazorHolEntraId`
6. Click Next
7. Use the following options:
   - Framework: .NET 8.0
   - Authentication Type: None
   - Configure for HTTPS: Checked
    - Progressive Web Application: Unchecked
    - Include sample pages: Checked
8. Click Create

## Referencing Packages

1. Edit the `BlazorHolEntraId.csproj` file:

```xml
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.8" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="8.0.8" PrivateAssets="all" />
    <PackageReference Include="Microsoft.Authentication.WebAssembly.Msal" Version="8.0.8" />
    <PackageReference Include="Microsoft.Graph" Version="5.56.0" />
  </ItemGroup>
```

## Registering Services

1. Open the `Program.cs` file
2. Add the following code:

```csharp
using BlazorHolEntraId;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes
            .Add("https://graph.microsoft.com/User.Read");
});

builder.Services.AddScoped(sp =>
{
    var authorizationMessageHandler =
        sp.GetRequiredService<AuthorizationMessageHandler>();
    authorizationMessageHandler.InnerHandler = new HttpClientHandler();
    authorizationMessageHandler.ConfigureHandler(
        authorizedUrls: new[] { "https://graph.microsoft.com/v1.0" },
        scopes: new[] { "User.Read" });

    return new HttpClient(authorizationMessageHandler);
});

await builder.Build().RunAsync();
```

## Update Routing

1. Open the `App.razor` file
2. Modify the content of the file to the following:

```html
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    @if (context.User.Identity?.IsAuthenticated != true)
                    {
                        <RedirectToLogin />
                    }
                    else
                    {
                        <p role="alert">You are not authorized to access this resource.</p>
                    }
                </NotAuthorized>
            </AuthorizeRouteView>
            <FocusOnNavigate RouteData="@routeData" Selector="h1" />
        </Found>
        <NotFound>
            <PageTitle>Not found</PageTitle>
            <LayoutView Layout="@typeof(MainLayout)">
                <p role="alert">Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

## Create Shared Components

1. Add a new Razor component named `LoginDisplay.razor` to the `Layout` folder with the following content:

```html
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication
@inject NavigationManager Navigation

<AuthorizeView>
    <Authorized>
        Hello, @context.User.Identity?.Name!
        <button class="nav-link btn btn-link" @onclick="BeginLogOut">Log out</button>
    </Authorized>
    <NotAuthorized>
        <a href="authentication/login">Log in</a>
    </NotAuthorized>
</AuthorizeView>

@code{
    public void BeginLogOut()
    {
        Navigation.NavigateToLogout("authentication/logout");
    }
}
```

2. Add a new Razor component named `RedirectToLogin.razor` to the `Layout` folder with the following content:

```html
@inject NavigationManager Navigation

@code {
    protected override void OnInitialized()
    {
        Navigation.NavigateTo($"authentication/login?returnUrl={Uri.EscapeDataString(Navigation.Uri)}");
    }
}
```

3. Edit `MainLayout.razor` file:

```html
@inherits LayoutComponentBase

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <div class="top-row px-4 auth">
            <LoginDisplay />
        </div>

        <article class="content px-4">
            @Body
        </article>
    </main>
</div>
```

## Create Authentication Page

1. Add a new Razor component named `Authentication.razor` to the `Pages` folder with the following content:

```html
@page "/authentication/{action}"

@using Microsoft.AspNetCore.Components.WebAssembly.Authentication

<RemoteAuthenticatorView Action="@Action" />

@code{
    [Parameter] public string? Action { get; set; }
}
```

## Display User Profile Information

1. Edit the `Home.razor` file:

```html
@page "/"

@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication
@using System.Text.Json

@inject HttpClient Http

<PageTitle>Index</PageTitle>

<h1>Blazor WebAssembly Entra Id Signin</h1>

<AuthorizeView>
    <NotAuthorized>
        <div>This page can be accessed by all users, authenticated or not.</div>
        <p>Click <a href="authentication/login">Log in</a> to sign into the application.</p>
    </NotAuthorized>
    <Authorized>
        @if (graphApiResponse != null)
        {
            <p>
                The app called Microsoft Graph's <code>/me</code> API for your user and received the
                following:
            </p>

            <p><pre><code class="language-js">@JsonSerializer.Serialize(graphApiResponse, new JsonSerializerOptions { WriteIndented = true })</code></pre></p>
        }

        @code {
        private JsonDocument? graphApiResponse = null;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                using var response = await Http.GetAsync("https://graph.microsoft.com/v1.0/me");
                response.EnsureSuccessStatusCode();
                graphApiResponse = await response.Content.ReadFromJsonAsync<JsonDocument>().ConfigureAwait(false);
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }
        }

        }
    </Authorized>
</AuthorizeView>
```

This code uses an authenticated HTTP client to call the Microsoft Graph API and display the user's profile information.

## Setting the Azure AD Configuration

1. Open the `wwwroot/appsettings.json` file
2. Add the configuration provided by your instructor.
3. 🛑 This information includes secrets and should not be checked into source control.

## Running the Application

1. Press F5 to run the application
2. The app should automatically attempt to log you in using the Entra Id authentication provider
3. If you are not logged in, you will be redirected to the Entra Id login page
4. After logging in, you will be redirected back to the app and see your user profile information
