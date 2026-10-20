using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Telemetry.Services;

/// <summary>
/// Custom ActivitySource and Meter for the server. The name matches the
/// application name, which ServiceDefaults registers with OpenTelemetry.
/// </summary>
public static class ServerDiagnostics
{
    public const string Name = "Telemetry";

    public static readonly ActivitySource ActivitySource = new(Name);
    public static readonly Meter Meter = new(Name);

    public static readonly Counter<long> ForecastsGenerated =
        Meter.CreateCounter<long>("weather.forecasts.generated", description: "Number of weather forecasts generated");
}
