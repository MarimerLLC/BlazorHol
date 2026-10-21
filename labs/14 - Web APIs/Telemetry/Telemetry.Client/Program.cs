using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Telemetry.Client.Observability;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// OpenTelemetry traces, metrics, and logs, sent to the server's /otlp proxy
builder.AddBrowserTelemetry();

// HttpClient for calling the server's Web API; requests are traced and carry traceparent
builder.Services.AddScoped(_ => new HttpClient(new TracingHandler { InnerHandler = new HttpClientHandler() })
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

var host = builder.Build();
host.StartBrowserTelemetry();
await host.RunAsync();
