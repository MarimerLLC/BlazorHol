using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using OpenTelemetry.Resources;

namespace Telemetry.Client.Observability;

/// <summary>
/// Holds OTLP/JSON items produced by the exporters until the
/// <see cref="TelemetryShipper"/> sends them to the server.
/// </summary>
/// <remarks>
/// OpenTelemetry exporters are synchronous, and the browser can't make
/// synchronous HTTP calls, so the exporters only convert and queue telemetry.
/// Sending happens asynchronously in the shipper.
/// </remarks>
public sealed class OtlpJsonBuffer(Resource resource)
{
    // Drop telemetry instead of growing forever if the server is unreachable
    private const int MaxItemsPerSignal = 5000;

    private readonly ConcurrentQueue<ScopedItem> spans = new();
    private readonly ConcurrentQueue<ScopedItem> logs = new();
    private readonly ConcurrentQueue<ScopedItem> metrics = new();

    public JsonObject Resource { get; } = new()
    {
        ["attributes"] = OtlpJson.Attributes(resource.Attributes.Select(a => new KeyValuePair<string, object?>(a.Key, a.Value)))
    };

    public void AddSpan(string scope, string? version, JsonObject span) => Enqueue(spans, new(scope, version, span));

    public void AddLog(string scope, string? version, JsonObject log) => Enqueue(logs, new(scope, version, log));

    public void AddMetric(string scope, string? version, JsonObject metric) => Enqueue(metrics, new(scope, version, metric));

    public IReadOnlyList<ScopedItem> TakeSpans() => Drain(spans);

    public IReadOnlyList<ScopedItem> TakeLogs() => Drain(logs);

    public IReadOnlyList<ScopedItem> TakeMetrics() => Drain(metrics);

    private static void Enqueue(ConcurrentQueue<ScopedItem> queue, ScopedItem item)
    {
        if (queue.Count < MaxItemsPerSignal)
            queue.Enqueue(item);
    }

    private static List<ScopedItem> Drain(ConcurrentQueue<ScopedItem> queue)
    {
        var items = new List<ScopedItem>();
        while (queue.TryDequeue(out var item))
            items.Add(item);
        return items;
    }

    public sealed record ScopedItem(string Scope, string? Version, JsonObject Item);
}
