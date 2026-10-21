using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Nodes;

namespace Telemetry.Client.Observability;

/// <summary>
/// Helpers for writing the OTLP/JSON encoding.
/// See https://opentelemetry.io/docs/specs/otlp/#json-protobuf-encoding
/// </summary>
internal static class OtlpJson
{
    public static string UnixNanos(DateTime utc) =>
        ((utc.Ticks - DateTime.UnixEpoch.Ticks) * 100).ToString(CultureInfo.InvariantCulture);

    public static string UnixNanos(DateTimeOffset time) => UnixNanos(time.UtcDateTime);

    // OTLP/JSON uses hex for trace and span ids (not base64 like regular protobuf JSON)
    public static string HexId(ActivityTraceId id) => id.ToHexString();

    public static string HexId(ActivitySpanId id) => id.ToHexString();

    public static JsonArray Attributes(IEnumerable<KeyValuePair<string, object?>> tags)
    {
        var attributes = new JsonArray();
        foreach (var (key, value) in tags)
        {
            if (value is not null)
                attributes.Add(new JsonObject { ["key"] = key, ["value"] = AnyValue(value) });
        }
        return attributes;
    }

    public static JsonObject AnyValue(object value) => value switch
    {
        string s => new JsonObject { ["stringValue"] = s },
        bool b => new JsonObject { ["boolValue"] = b },
        // OTLP/JSON encodes 64-bit integers as strings
        sbyte or byte or short or ushort or int or uint or long =>
            new JsonObject { ["intValue"] = Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) },
        float or double or decimal =>
            new JsonObject { ["doubleValue"] = Convert.ToDouble(value, CultureInfo.InvariantCulture) },
        IEnumerable items => new JsonObject
        {
            ["arrayValue"] = new JsonObject
            {
                ["values"] = new JsonArray(items.Cast<object?>().OfType<object>().Select(i => (JsonNode)AnyValue(i)).ToArray())
            }
        },
        _ => new JsonObject { ["stringValue"] = Convert.ToString(value, CultureInfo.InvariantCulture) }
    };

    public static JsonObject Scope(string name, string? version)
    {
        var scope = new JsonObject { ["name"] = name };
        if (!string.IsNullOrEmpty(version))
            scope["version"] = version;
        return scope;
    }
}
