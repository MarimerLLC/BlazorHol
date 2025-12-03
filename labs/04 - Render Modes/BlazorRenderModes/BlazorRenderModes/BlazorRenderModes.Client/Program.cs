using Marimer.Blazor.RenderMode.WebAssembly;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddRenderModeDetection();

await builder.Build().RunAsync();

