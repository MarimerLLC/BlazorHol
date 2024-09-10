# Authorization

## Using the `BlazorHolAuthentication` app

1. Open the `BlazorHolAuthentication` app from lab 06.

## Using the Authorization Namespace

1. Open the `_Imports.razor` file.
1. Add the `Microsoft.AspNetCore.Authorization` namespace.

```html
@using Microsoft.AspNetCore.Authorization
```

## Displaying User Information

1. Open the `Home.razor` file.
1. Inject the `AuthenticationStateProvider` service.

```csharp
@inject AuthenticationStateProvider AuthenticationStateProvider
```

3. Add the `AuthenticationState` property to the `Home` component.

```csharp
    private AuthenticationState AuthenticationState { get; set; }
```

4. Add the `OnInitializedAsync` method to the `Home` component.

```csharp
    protected override async Task OnInitializedAsync()
    {
        AuthenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
    }
```

5. Use the `AuthenticationState` property in the `Home` component.

```html
<div class="border border-primary">
    @if (AuthenticationState.User.Identity.IsAuthenticated)
    {
        <p>You are authenticated.</p>
        <p>Username: @AuthenticationState.User.Identity.Name</p>
        @foreach (var claim in AuthenticationState.User.Claims)
        {
            <p>@claim.Type: @claim.Value</p>
        }
        <p>Is Admin: @AuthenticationState.User.IsInRole("Admin")</p>
    }
    else
    {
        <p>You are not authenticated.</p>
    }
</div>
```

6. Press `F5` to run the app.
7. Ensure you are logged out.
8. You should see a message indicating you are not authenticated.
9. Click the login link.
10. You should be redirected to the login page.
11. Enter your credentials.
12. You should be redirected back to the home page.
13. You should see a message indicating you are authenticated.

## Using the Authorize Attribute

1. Open the `Pages/Counter.razor` file.
1. Add the `[Authorize]` attribute to the `Counter` component.

```html
@page "/counter"
@rendermode InteractiveServer

@attribute [Authorize]
```

3. Press `F5` to run the app.
4. Ensure you are logged out.
5. Navigate to the `Counter` page.
6. You should see an error 404 message.

The `Routes.razor` file uses the `AuthorizeRouteView` component to display the `Counter` page only when the user is authenticated.

## Using the AuthorizeView Component

1. Open the `Pages/Counter.razor` file.
2. Remove the `[Authorize]` attribute from the `Counter` component.
3. Add the `AuthorizeView` component to the `Counter` component.

```html
@page "/counter"
@rendermode InteractiveServer

<PageTitle>Counter</PageTitle>

<h1>Counter</h1>

<p role="status">Current count: @currentCount</p>

<AuthorizeView>
    <Authorized>
        <button class="btn btn-primary" @onclick="IncrementCount">Click me</button>
    </Authorized>
    <NotAuthorized>
        <p><a href="/login?ret=/counter">Login</a> to change values</p>
    </NotAuthorized>
</AuthorizeView>

@code {
    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }
}
```

4. Enhance the `Login.razor` component to accept the `ret` parameter.

Add the following property to the `Login` component.

```csharp
    private string ReturnUrl { get; set; } = "/";
```

Add an `OnInitialized` method to the `Login` component.

```csharp
    protected override void OnInitialized()
    {
        var query = new Uri(NavigationManager.Uri).Query;
        if (!string.IsNullOrEmpty(query))
        {
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(query);
            ReturnUrl = queryDictionary["ret"] ?? "/";
        }
    }
```

Use the `ReturnUrl` property in the `Login` component.

```csharp
    NavigationManager.NavigateTo(ReturnUrl);
```

5. Press `F5` to run the app.
6. Ensure you are logged out.
7. Navigate to the `Counter` page.
8. You should see a login link.
9. Click the login link.
10. You should be redirected to the login page.
11. Enter your credentials.
12. You should be redirected back to the `Counter` page.

## Using Role-based Authorization

1. Open the `Counter.razor` file.
2. Add a `RequiredRole` property to the `Counter` component.

```csharp
    private string RequiredRole { get; set; } = "Admin";
```

3. Add a role to the `AuthorizeView` component.

```html
<AuthorizeView Roles="@RequiredRole">
    <Authorized>
        <button class="btn btn-primary" @onclick="IncrementCount">Click me</button>
    </Authorized>
    <NotAuthorized>
        <p><a href="/login?ret=/counter">Login</a> as an @RequiredRole to change values</p>
    </NotAuthorized>
</AuthorizeView>
```

4. Press `F5` to run the app.
5. Log in and out and see how the `Counter` page changes based on your role.
6. Edit the `RequiredRole` property to see how the `Counter` page changes based on the role. Try `Admin` and `User`.

## Using Policy-based Authorization

1. Open the `Program.cs` file.
2. Add an authorization policy to the `services.AddAuthorization` method.

```csharp
builder.Services.AddAuthorization(c => c
    .AddPolicy("IsAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "Admin")));
```

3. Open the `Counter.razor` file.
4. Change the `AuthorizeView` component to use the `IsAdmin` policy.

```html
<AuthorizeView Policy="IsAdmin">
```

5. Press `F5` to run the app.
6. Log in and out and see how the `Counter` page changes based on your role.
