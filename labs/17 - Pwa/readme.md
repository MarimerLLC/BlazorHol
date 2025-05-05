# Creating a Blazor PWA App

## Creating the Solution

1. Open Visual Studio
2. Click on Create a new project
3. Select Standalone WebAssembly Blazor App
4. Click Next
5. Enter the project name: `BlazorHolPwa`
6. Click Next
7. Use the following options:
   - Target Framework: .NET 8.0
   - Authentication Type: None
   - Configure for HTTPS: Checked
   - Progressive Web Application: Checked
   - Include sample pages: Checked
8. Click Create

## Examine the csproj File

1. Open the `BlazorHolPwa.csproj` file
2. Notice the `<ServiceWorkerAssetsManifest>service-worker-assets.js</ServiceWorkerAssetsManifest>` element
3. Notice the service worker file:

```xml
  <ItemGroup>
    <ServiceWorker Include="wwwroot\service-worker.js" PublishedContent="wwwroot\service-worker.published.js" />
  </ItemGroup>
```

## Examine the Service Worker

1. Open the `wwwroot\service-worker.js` file
2. Notice how the _development_ service worker doesn't support offline mode
3. Open the `wwwroot\service-worker.published.js` file
4. Notice how the _published_ service worker supports offline mode

## Using Local Storage

1. Add a reference to the `Blazored.LocalStorage` NuGet package (https://github.com/Blazored/LocalStorage)
2. Register the `LocalStorageService` in the `Program.cs` file:

```csharp
builder.Services.AddBlazoredLocalStorage();
```

3. Use Local Storage in `Home.razor`:

```html
@page "/"

@using System.ComponentModel.DataAnnotations
@using Blazored.LocalStorage

@inject ILocalStorageService LocalStorage

<PageTitle>Home</PageTitle>

<h1>Hello, world!</h1>

@if (myInfo != null)
{
    <EditForm Model="@myInfo" OnValidSubmit="HandleValidSubmit">
        <DataAnnotationsValidator />
        <ValidationSummary />

        <InputText @bind-Value="@myInfo.Name" />
        <InputNumber @bind-Value="@myInfo.Age" />
        <button type="submit">Submit</button>
    </EditForm>
}

<p class="alert-info">@message</p>

@code {
    private MyInfo? myInfo = new MyInfo();
    private string message = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        myInfo = await LocalStorage.GetItemAsync<MyInfo>("myInfo");
        message = myInfo == null ? "No data found in local storage." : "Data loaded from local storage";
        if (myInfo == null)
        {
            myInfo = new MyInfo();
        }
    }

    private async void HandleValidSubmit()
    {
        await LocalStorage.SetItemAsync("myInfo", myInfo);
        message = "Data saved to local storage.";
    }

    private class MyInfo
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
```

## Running the App

1. Run the app
2. Add some data
3. Click Save
4. Open the browser's developer tools
5. Go to the Application tab
6. Click on Local Storage
7. Notice the data saved in local storage
