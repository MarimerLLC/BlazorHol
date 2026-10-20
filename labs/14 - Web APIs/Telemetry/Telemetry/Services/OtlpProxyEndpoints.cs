using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;

namespace Telemetry.Services;

/// <summary>
/// Receives OTLP/HTTP requests from the WebAssembly client and forwards them,
/// unchanged, to the OTLP endpoint Aspire configured for this server
/// (the Aspire dashboard). The browser never talks to the collector directly,
/// so the dashboard's API key and address stay on the server.
/// </summary>
public static class OtlpProxyEndpoints
{
    public const string HttpClientName = "otlp-proxy";
    private const int MaxRequestBytes = 1024 * 1024;
    private static readonly string[] Signals = ["traces", "metrics", "logs"];
    private static readonly string[] ContentTypes = ["application/json", "application/x-protobuf"];

    public static IServiceCollection AddOtlpProxy(this IServiceCollection services)
    {
        services.AddHttpClient(HttpClientName);
        return services;
    }

    // NOTE: This endpoint is anonymous to keep the demo simple. In a real app you
    // would want to protect it (authentication, rate limiting, origin checks),
    // because anyone who can reach it can write data into your telemetry backend.
    public static IEndpointRouteBuilder MapOtlpProxy(this IEndpointRouteBuilder app)
    {
        app.MapPost("/otlp/v1/{signal}", ForwardAsync)
            .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBytes))
            .DisableAntiforgery();
        return app;
    }

    private static async Task ForwardAsync(
        string signal,
        HttpContext context,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory)
    {
        var request = context.Request;
        var response = context.Response;

        // This is an API endpoint; don't let the app's status code pages render HTML
        if (context.Features.Get<IStatusCodePagesFeature>() is { } statusCodePages)
            statusCodePages.Enabled = false;

        var contentType = request.ContentType?.Split(';')[0].Trim();
        if (!Signals.Contains(signal) || contentType is null || !ContentTypes.Contains(contentType))
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        // Aspire injects these when the app is launched by the AppHost
        var endpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return;
        }

        var upstream = new HttpRequestMessage(HttpMethod.Post, $"{endpoint.TrimEnd('/')}/v1/{signal}")
        {
            Content = new StreamContent(request.Body)
        };
        upstream.Content.Headers.TryAddWithoutValidation("Content-Type", request.ContentType);
        foreach (var (name, value) in ParseHeaders(configuration["OTEL_EXPORTER_OTLP_HEADERS"]))
            upstream.Headers.TryAddWithoutValidation(name, value);

        try
        {
            // Don't trace the proxy's own outbound call; it would just be noise
            using var suppress = SuppressInstrumentationScope.Begin();

            var client = httpClientFactory.CreateClient(HttpClientName);
            using var upstreamResponse = await client.SendAsync(upstream, context.RequestAborted);

            response.StatusCode = (int)upstreamResponse.StatusCode;
            response.ContentType = upstreamResponse.Content.Headers.ContentType?.ToString();
            await upstreamResponse.Content.CopyToAsync(response.Body, context.RequestAborted);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            loggerFactory.CreateLogger(typeof(OtlpProxyEndpoints))
                .LogWarning(ex, "Failed to forward browser {Signal} to {Endpoint}", signal, endpoint);
            response.StatusCode = StatusCodes.Status502BadGateway;
        }
    }

    // OTEL_EXPORTER_OTLP_HEADERS is a comma-separated list of key=value pairs
    private static IEnumerable<(string Name, string Value)> ParseHeaders(string? headers)
    {
        if (string.IsNullOrWhiteSpace(headers))
            yield break;

        foreach (var pair in headers.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var index = pair.IndexOf('=');
            if (index > 0)
                yield return (pair[..index].Trim(), Uri.UnescapeDataString(pair[(index + 1)..].Trim()));
        }
    }
}
