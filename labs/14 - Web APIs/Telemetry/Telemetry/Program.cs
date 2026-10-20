using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Telemetry.Client.Observability;
using Telemetry.Client.Pages;
using Telemetry.Components;
using Telemetry.Services;

var builder = WebApplication.CreateBuilder(args);

// OpenTelemetry, health checks, service discovery, and resilience (from Aspire ServiceDefaults)
builder.AddServiceDefaults();

// Components from the client project (such as InteractiveAuto pages) also run on the
// server, so listen to their ActivitySource and Meter here as well
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(ClientDiagnostics.Name))
    .WithMetrics(metrics => metrics.AddMeter(ClientDiagnostics.Name));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<WeatherService>();
builder.Services.AddOtlpProxy();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

// Web API called by the WebAssembly client
app.MapGet("/api/weather", (WeatherService weatherService, int days = 5) =>
    weatherService.GetForecastsAsync(days, "wasm-client"));

// Proxy that forwards OTLP telemetry from the WebAssembly client to the Aspire dashboard
app.MapOtlpProxy();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Telemetry.Client._Imports).Assembly);

app.Run();
