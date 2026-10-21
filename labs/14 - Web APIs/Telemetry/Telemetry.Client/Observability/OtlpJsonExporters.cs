using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;

namespace Telemetry.Client.Observability;

/// <summary>
/// Converts finished spans (Activity objects) to OTLP/JSON.
/// </summary>
public sealed class OtlpJsonTraceExporter(OtlpJsonBuffer buffer) : BaseExporter<Activity>
{
    public override ExportResult Export(in Batch<Activity> batch)
    {
        foreach (var activity in batch)
        {
            var span = new JsonObject
            {
                ["traceId"] = OtlpJson.HexId(activity.TraceId),
                ["spanId"] = OtlpJson.HexId(activity.SpanId),
                ["name"] = activity.DisplayName,
                // ActivityKind is zero-based; OTLP SpanKind reserves 0 for "unspecified"
                ["kind"] = (int)activity.Kind + 1,
                ["startTimeUnixNano"] = OtlpJson.UnixNanos(activity.StartTimeUtc),
                ["endTimeUnixNano"] = OtlpJson.UnixNanos(activity.StartTimeUtc + activity.Duration),
                ["attributes"] = OtlpJson.Attributes(activity.TagObjects),
                // ActivityStatusCode and OTLP StatusCode use the same values
                ["status"] = new JsonObject
                {
                    ["code"] = (int)activity.Status,
                    ["message"] = activity.StatusDescription ?? string.Empty
                },
                ["events"] = new JsonArray(activity.Events.Select(e => (JsonNode)new JsonObject
                {
                    ["name"] = e.Name,
                    ["timeUnixNano"] = OtlpJson.UnixNanos(e.Timestamp),
                    ["attributes"] = OtlpJson.Attributes(e.Tags)
                }).ToArray())
            };

            if (activity.ParentSpanId != default)
                span["parentSpanId"] = OtlpJson.HexId(activity.ParentSpanId);
            if (!string.IsNullOrEmpty(activity.TraceStateString))
                span["traceState"] = activity.TraceStateString;

            buffer.AddSpan(activity.Source.Name, activity.Source.Version, span);
        }
        return ExportResult.Success;
    }
}

/// <summary>
/// Converts ILogger log records to OTLP/JSON.
/// </summary>
public sealed class OtlpJsonLogExporter(OtlpJsonBuffer buffer) : BaseExporter<LogRecord>
{
    public override ExportResult Export(in Batch<LogRecord> batch)
    {
        foreach (var record in batch)
        {
            // LogRecord instances are pooled, so copy everything we need now
            var attributes = (record.Attributes ?? [])
                .Where(a => a.Key != "{OriginalFormat}")
                .ToList();
            if (record.EventId.Id != 0)
                attributes.Add(new("event.id", record.EventId.Id));
            if (!string.IsNullOrEmpty(record.EventId.Name))
                attributes.Add(new("event.name", record.EventId.Name));
            if (record.Exception is { } ex)
            {
                attributes.Add(new("exception.type", ex.GetType().FullName));
                attributes.Add(new("exception.message", ex.Message));
                attributes.Add(new("exception.stacktrace", ex.ToString()));
            }

            var log = new JsonObject
            {
                ["timeUnixNano"] = OtlpJson.UnixNanos(record.Timestamp),
                ["observedTimeUnixNano"] = OtlpJson.UnixNanos(DateTime.UtcNow),
                ["severityNumber"] = SeverityNumber(record.LogLevel),
                ["severityText"] = record.LogLevel.ToString(),
                ["body"] = new JsonObject { ["stringValue"] = record.FormattedMessage ?? record.Body ?? string.Empty },
                ["attributes"] = OtlpJson.Attributes(attributes)
            };

            // Correlate the log with the span that was active when it was written
            if (record.TraceId != default)
            {
                log["traceId"] = OtlpJson.HexId(record.TraceId);
                log["spanId"] = OtlpJson.HexId(record.SpanId);
                log["flags"] = (int)record.TraceFlags;
            }

            buffer.AddLog(record.CategoryName ?? "default", null, log);
        }
        return ExportResult.Success;
    }

    private static int SeverityNumber(LogLevel level) => level switch
    {
        LogLevel.Trace => 1,
        LogLevel.Debug => 5,
        LogLevel.Information => 9,
        LogLevel.Warning => 13,
        LogLevel.Error => 17,
        LogLevel.Critical => 21,
        _ => 0
    };
}

/// <summary>
/// Converts metrics collected by the OpenTelemetry SDK to OTLP/JSON.
/// </summary>
public sealed class OtlpJsonMetricExporter(OtlpJsonBuffer buffer) : BaseExporter<Metric>
{
    public override ExportResult Export(in Batch<Metric> batch)
    {
        foreach (var metric in batch)
        {
            var json = new JsonObject
            {
                ["name"] = metric.Name,
                ["description"] = metric.Description,
                ["unit"] = metric.Unit
            };

            // OTLP AggregationTemporality: 1 = delta, 2 = cumulative
            var temporality = metric.Temporality == AggregationTemporality.Delta ? 1 : 2;
            var dataPoints = new JsonArray();

            switch (metric.MetricType)
            {
                case MetricType.LongSum:
                case MetricType.LongSumNonMonotonic:
                case MetricType.DoubleSum:
                case MetricType.DoubleSumNonMonotonic:
                    var isLong = metric.MetricType is MetricType.LongSum or MetricType.LongSumNonMonotonic;
                    foreach (ref readonly var point in metric.GetMetricPoints())
                    {
                        var dataPoint = NumberDataPoint(point);
                        if (isLong)
                            dataPoint["asInt"] = point.GetSumLong().ToString(CultureInfo.InvariantCulture);
                        else
                            dataPoint["asDouble"] = point.GetSumDouble();
                        dataPoints.Add(dataPoint);
                    }
                    json["sum"] = new JsonObject
                    {
                        ["dataPoints"] = dataPoints,
                        ["aggregationTemporality"] = temporality,
                        ["isMonotonic"] = metric.MetricType is MetricType.LongSum or MetricType.DoubleSum
                    };
                    break;

                case MetricType.LongGauge:
                case MetricType.DoubleGauge:
                    foreach (ref readonly var point in metric.GetMetricPoints())
                    {
                        var dataPoint = NumberDataPoint(point);
                        if (metric.MetricType == MetricType.LongGauge)
                            dataPoint["asInt"] = point.GetGaugeLastValueLong().ToString(CultureInfo.InvariantCulture);
                        else
                            dataPoint["asDouble"] = point.GetGaugeLastValueDouble();
                        dataPoints.Add(dataPoint);
                    }
                    json["gauge"] = new JsonObject { ["dataPoints"] = dataPoints };
                    break;

                case MetricType.Histogram:
                    foreach (ref readonly var point in metric.GetMetricPoints())
                    {
                        var dataPoint = NumberDataPoint(point);
                        dataPoint["count"] = point.GetHistogramCount().ToString(CultureInfo.InvariantCulture);
                        dataPoint["sum"] = point.GetHistogramSum();
                        if (point.TryGetHistogramMinMaxValues(out var min, out var max))
                        {
                            dataPoint["min"] = min;
                            dataPoint["max"] = max;
                        }

                        var bucketCounts = new JsonArray();
                        var explicitBounds = new JsonArray();
                        foreach (var bucket in point.GetHistogramBuckets())
                        {
                            bucketCounts.Add(bucket.BucketCount.ToString(CultureInfo.InvariantCulture));
                            // The last bucket's bound is +Infinity, which OTLP leaves implicit
                            if (!double.IsPositiveInfinity(bucket.ExplicitBound))
                                explicitBounds.Add(bucket.ExplicitBound);
                        }
                        dataPoint["bucketCounts"] = bucketCounts;
                        dataPoint["explicitBounds"] = explicitBounds;
                        dataPoints.Add(dataPoint);
                    }
                    json["histogram"] = new JsonObject
                    {
                        ["dataPoints"] = dataPoints,
                        ["aggregationTemporality"] = temporality
                    };
                    break;

                default:
                    // Exponential histograms aren't used in this demo
                    continue;
            }

            buffer.AddMetric(metric.MeterName, metric.MeterVersion, json);
        }
        return ExportResult.Success;
    }

    private static JsonObject NumberDataPoint(in MetricPoint point)
    {
        var tags = new List<KeyValuePair<string, object?>>();
        foreach (var tag in point.Tags)
            tags.Add(tag);

        return new JsonObject
        {
            ["attributes"] = OtlpJson.Attributes(tags),
            ["startTimeUnixNano"] = OtlpJson.UnixNanos(point.StartTime),
            ["timeUnixNano"] = OtlpJson.UnixNanos(point.EndTime)
        };
    }
}
