using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using OpenTelemetry.Metrics;

namespace Telemetry.Client.Observability;

/// <summary>
/// Periodically sends buffered telemetry to the server's /otlp proxy using
/// OTLP/HTTP with JSON encoding. This takes the place of the batch processor
/// and OTLP exporter a normal .NET app would use, neither of which works in
/// the single-threaded browser runtime.
/// </summary>
public sealed class TelemetryShipper(HttpClient httpClient, OtlpJsonBuffer buffer, MetricReader metricReader)
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    private readonly SemaphoreSlim flushLock = new(1, 1);
    private PeriodicTimer? timer;

    public int SpansSent { get; private set; }
    public int LogsSent { get; private set; }
    public int MetricExports { get; private set; }
    public string? LastError { get; private set; }
    public DateTime? LastFlush { get; private set; }

    public event Action? Flushed;

    public void Start()
    {
        if (timer is not null)
            return;

        timer = new PeriodicTimer(Interval);
        _ = RunAsync(timer);
    }

    private async Task RunAsync(PeriodicTimer periodicTimer)
    {
        while (await periodicTimer.WaitForNextTickAsync())
            await FlushAsync();
    }

    public async Task FlushAsync()
    {
        await flushLock.WaitAsync();
        try
        {
            // Pull current metric values into the buffer
            metricReader.Collect();

            var spans = buffer.TakeSpans();
            var logs = buffer.TakeLogs();
            var metrics = buffer.TakeMetrics();

            if (await SendAsync("traces", "resourceSpans", "scopeSpans", "spans", spans))
                SpansSent += spans.Count;
            if (await SendAsync("logs", "resourceLogs", "scopeLogs", "logRecords", logs))
                LogsSent += logs.Count;
            if (await SendAsync("metrics", "resourceMetrics", "scopeMetrics", "metrics", metrics))
                MetricExports++;

            LastFlush = DateTime.Now;
        }
        finally
        {
            flushLock.Release();
        }
        Flushed?.Invoke();
    }

    private async Task<bool> SendAsync(
        string signal, string resourceKey, string scopeKey, string itemsKey, IReadOnlyList<OtlpJsonBuffer.ScopedItem> items)
    {
        if (items.Count == 0)
            return false;

        // Build an Export{Signal}ServiceRequest: one resource, items grouped by instrumentation scope
        var scopes = new JsonArray();
        foreach (var group in items.GroupBy(i => (i.Scope, i.Version)))
        {
            scopes.Add(new JsonObject
            {
                ["scope"] = OtlpJson.Scope(group.Key.Scope, group.Key.Version),
                [itemsKey] = new JsonArray(group.Select(i => (JsonNode)i.Item).ToArray())
            });
        }

        var request = new JsonObject
        {
            [resourceKey] = new JsonArray(new JsonObject
            {
                ["resource"] = buffer.Resource.DeepClone(),
                [scopeKey] = scopes
            })
        };

        try
        {
            var content = new StringContent(request.ToJsonString(), Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using var response = await httpClient.PostAsync($"otlp/v1/{signal}", content);
            if (!response.IsSuccessStatusCode)
            {
                LastError = $"{signal}: {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}";
                return false;
            }
            LastError = null;
            return true;
        }
        catch (Exception ex)
        {
            // Use the console, not ILogger: logging here would create more telemetry to ship
            LastError = $"{signal}: {ex.Message}";
            Console.WriteLine($"Failed to send {signal}: {ex.Message}");
            return false;
        }
    }
}
