using System.Diagnostics;

namespace Telemetry.Client.Observability;

/// <summary>
/// Creates a client span for each outgoing HTTP request and adds the W3C
/// traceparent header, so the server's span joins the same distributed trace.
/// </summary>
public sealed class TracingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var activity = ClientDiagnostics.ActivitySource.StartActivity(
            $"{request.Method} {request.RequestUri?.AbsolutePath}", ActivityKind.Client);

        if (activity is not null)
        {
            activity.SetTag("http.request.method", request.Method.Method);
            activity.SetTag("url.full", request.RequestUri?.ToString());
            activity.SetTag("server.address", request.RequestUri?.Host);
            activity.SetTag("server.port", request.RequestUri?.Port);

            // Writes traceparent (and tracestate/baggage) using the current propagator
            DistributedContextPropagator.Current.Inject(activity, request, static (carrier, name, value) =>
            {
                var message = (HttpRequestMessage)carrier!;
                message.Headers.Remove(name);
                message.Headers.TryAddWithoutValidation(name, value);
            });
        }

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            activity?.SetTag("http.response.status_code", (int)response.StatusCode);
            if (!response.IsSuccessStatusCode)
                activity?.SetStatus(ActivityStatusCode.Error);
            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }
}
