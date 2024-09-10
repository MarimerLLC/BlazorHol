using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Marimer.Blazor.RenderMode.WebAssembly;
using BlazorHolState.Client;
using BlazorHolState;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddTransient<HttpClient>(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddRenderModeDetection();

builder.Services.AddSingleton(typeof(ISessionManager), typeof(SessionManager));

await builder.Build().RunAsync();
