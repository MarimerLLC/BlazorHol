using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Telemetry.Client.Observability;

public static class BrowserTelemetryExtensions
{
    /// <summary>
    /// Configures the OpenTelemetry SDK for traces, metrics, and logs in the
    /// browser, exporting through the server's /otlp proxy.
    /// </summary>
    public static WebAssemblyHostBuilder AddBrowserTelemetry(this WebAssemblyHostBuilder builder)
    {
        // Each browser tab is its own service instance in the dashboard.
        // CreateEmpty skips the default resource detectors, which use APIs the browser doesn't support.
        var resourceBuilder = ResourceBuilder.CreateEmpty()
            .AddService(ClientDiagnostics.ServiceName, serviceInstanceId: Guid.NewGuid().ToString())
            .AddTelemetrySdk()
            .AddAttributes([new("deployment.environment.name", builder.HostEnvironment.Environment)]);

        var buffer = new OtlpJsonBuffer(resourceBuilder.Build());

        // Simple processors export each item immediately (into the buffer). The batch
        // processor and periodic metric reader need background threads, which the
        // browser doesn't have.
        var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddSource(ClientDiagnostics.Name)
            .AddProcessor(new SimpleActivityExportProcessor(new OtlpJsonTraceExporter(buffer)))
            .Build();

        var metricReader = new BaseExportingMetricReader(new OtlpJsonMetricExporter(buffer));
        var meterProvider = Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddMeter(ClientDiagnostics.Name)
            .AddReader(metricReader)
            .Build();

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.SetResourceBuilder(resourceBuilder);
            logging.IncludeFormattedMessage = true;
            logging.AddProcessor(new SimpleLogRecordExportProcessor(new OtlpJsonLogExporter(buffer)));
        });

        // Keep the providers alive for the lifetime of the app
        builder.Services.AddSingleton(tracerProvider);
        builder.Services.AddSingleton(meterProvider);
        builder.Services.AddSingleton(buffer);

        // The shipper's HttpClient is deliberately not traced
        var baseAddress = new Uri(builder.HostEnvironment.BaseAddress);
        builder.Services.AddSingleton(_ =>
            new TelemetryShipper(new HttpClient { BaseAddress = baseAddress }, buffer, metricReader));

        return builder;
    }

    /// <summary>
    /// Starts sending telemetry. WebAssembly apps don't run hosted services,
    /// so this is called explicitly after the host is built.
    /// </summary>
    public static WebAssemblyHost StartBrowserTelemetry(this WebAssemblyHost host)
    {
        host.Services.GetRequiredService<TelemetryShipper>().Start();
        return host;
    }
}
