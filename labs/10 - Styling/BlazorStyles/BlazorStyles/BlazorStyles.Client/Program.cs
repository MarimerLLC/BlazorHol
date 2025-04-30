using BlazorStyles.Client.Services;
using Marimer.Blazor.RenderMode.WebAssembly;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<AuthenticationStateProvider, RemoteAuthenticationStateProvider>();

builder.Services.AddRenderModeDetection();

await builder.Build().RunAsync();
