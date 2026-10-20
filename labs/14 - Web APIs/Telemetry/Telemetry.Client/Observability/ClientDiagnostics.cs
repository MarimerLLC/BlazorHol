using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Telemetry.Client.Observability;

/// <summary>
/// Custom ActivitySource and Meter for the WebAssembly client.
/// These are standard .NET APIs; OpenTelemetry listens to them.
/// </summary>
public static class ClientDiagnostics
{
    public const string ServiceName = "blazor-wasm";
    public const string Name = "Telemetry.Client";

    public static readonly ActivitySource ActivitySource = new(Name);
    public static readonly Meter Meter = new(Name);

    public static readonly Counter<long> ButtonClicks =
        Meter.CreateCounter<long>("demo.button.clicks", description: "Number of demo buttons clicked");

    public static readonly Histogram<double> WeatherLoadDuration =
        Meter.CreateHistogram<double>("demo.weather.load.duration", unit: "ms", description: "Time to load weather from the server API");
}
