# Blazor Telemetry with OpenTelemetry and Aspire

This demo shows OpenTelemetry (OTEL) traces, metrics, and logs coming from both sides of a Blazor Web App, all viewed in the Aspire dashboard:

- **Server**: the Blazor server project is instrumented using Aspire ServiceDefaults (ASP.NET Core, HttpClient, and runtime instrumentation, plus custom spans, metrics, and logs).
- **WebAssembly client**: an `InteractiveWebAssembly` page uses the OpenTelemetry .NET SDK in the browser. Its telemetry is posted to an `/otlp` proxy endpoint on the server, which forwards it to the Aspire dashboard.

Because the client sends a W3C `traceparent` header on its API calls, a single trace covers the browser and the server:

```text
Load weather              blazor-wasm    (custom span in the browser)
└─ GET /api/weather       blazor-wasm    (HTTP client span in the browser)
   └─ GET /api/weather    blazor-server  (ASP.NET Core instrumentation)
      └─ Generate forecasts  blazor-server  (custom span on the server)
```

## Projects

| Project | Purpose |
| --- | --- |
| `Telemetry.AppHost` | Aspire AppHost. Launches the Blazor app and the Aspire dashboard. |
| `Telemetry.ServiceDefaults` | Standard Aspire service defaults (OpenTelemetry, health checks, resilience, service discovery). |
| `Telemetry` | Blazor server project. Hosts the `/api/weather` Web API and the `/otlp` telemetry proxy. |
| `Telemetry.Client` | Blazor WebAssembly project. Contains the browser OpenTelemetry setup and the **Client Telemetry** page. |

## Prerequisites

- .NET 10 SDK
- A trusted HTTPS development certificate (`dotnet dev-certs https --trust`)
- No container runtime is needed; the app has no container resources.

## Running the demo

From Visual Studio, set `Telemetry.AppHost` as the startup project and press F5.

From the command line:

```bash
dotnet run --project Telemetry.AppHost
```

or with the Aspire CLI:

```bash
aspire run
```

The Aspire dashboard opens in your browser (the console output includes a login link with a token). On the **Resources** page, open the `blazor-server` endpoint to get to the app.

## Things to try

1. **Counter** page (`InteractiveAuto`). On the first visit, the component runs on the **Server** while the WebAssembly runtime downloads in the background. Click a few times, then reload the page; it now runs in **WebAssembly**. Click again. The page shows which mode it's in. In the dashboard, the `Increment counter` spans, the `demo.button.clicks` metric, and the `Counter incremented to N on {RenderMode}` logs show up under `blazor-server` for the first clicks and under `blazor-wasm` after the reload, all from the same component code. The count starts over after the switch because component state isn't carried across render modes.
2. **Weather** page (server-rendered). Loading the page creates a `Generate forecasts` span, a `weather.forecasts.generated` metric, and a log entry on `blazor-server`.
3. **Client Telemetry** page (WebAssembly):
   - **Load weather from API**: a browser span that calls the server API. Copy the trace id shown on the page and look it up on the dashboard's **Traces** page to see the client and server spans together.
   - **Do client-side work**: a browser span with nested child spans and an event.
   - **Fail an operation**: a span with error status and an exception, plus an error log.
   - **Send telemetry now**: flushes right away instead of waiting for the 5-second timer.
4. In the dashboard:
   - **Traces**: filter by the `blazor-wasm` resource, or open a trace to see both resources in one waterfall.
   - **Structured logs**: browser logs written with `ILogger` appear under `blazor-wasm` and link to their traces.
   - **Metrics**: select the `blazor-wasm` resource to see `demo.button.clicks` and `demo.weather.load.duration`.

Each browser tab has its own `service.instance.id`, so several tabs show up as separate instances of `blazor-wasm`.

## How it works

### Server

`Program.cs` calls `builder.AddServiceDefaults()`. That sets up OpenTelemetry and exports to the endpoint in `OTEL_EXPORTER_OTLP_ENDPOINT`, which the AppHost injects. The custom `ActivitySource` and `Meter` in `Services/ServerDiagnostics.cs` use the application name, which ServiceDefaults registers.

Components in the client project can also run on the server (prerendering, and `InteractiveAuto` before WebAssembly is ready). So the server also subscribes to the client's `ActivitySource` and `Meter` (`ClientDiagnostics.Name`) in `Program.cs`. When a component runs on the server, its telemetry goes through the server's pipeline; when it runs in the browser, it goes through the proxy. The component code is the same either way. `Counter.razor` adds a `blazor.render_mode` tag (from `RendererInfo.Name`) so you can tell the two apart.

In `AppHost.cs`, the app is configured with `.WithOtlpExporter(OtlpProtocol.HttpProtobuf)`. This points the injected OTLP endpoint at the dashboard's OTLP/**HTTP** listener (rather than gRPC), so the proxy can forward the browser's OTLP/HTTP requests to it as-is. The `https` and `http` profiles in `Telemetry.AppHost/Properties/launchSettings.json` set `ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL` to turn that listener on.

### The OTLP proxy

`Services/OtlpProxyEndpoints.cs` maps `POST /otlp/v1/{traces|metrics|logs}`. It:

1. Validates the signal name and content type, and limits the request size (1 MB).
2. Forwards the body unchanged to `{OTEL_EXPORTER_OTLP_ENDPOINT}/v1/{signal}`.
3. Adds the headers from `OTEL_EXPORTER_OTLP_HEADERS` (this is the dashboard's API key, which therefore never reaches the browser).
4. Returns the dashboard's response to the browser.

The proxy's own requests are left out of server tracing so they don't clutter the dashboard.

> The proxy is anonymous to keep the demo simple. In a real app, protect it (authentication, rate limiting, origin checks), because anyone who can reach it can write data into your telemetry backend.

### WebAssembly client

The client uses the standard .NET telemetry APIs (`ActivitySource`, `Meter`, `ILogger`) along with the OpenTelemetry SDK. Everything is set up in `Observability/BrowserTelemetryExtensions.cs`.

The browser runtime is single-threaded and can't make synchronous HTTP calls, so the usual OpenTelemetry setup doesn't work there:

- The **batch export processor** and **periodic metric reader** start background threads.
- The **OTLP exporter** sends synchronously.

So the client keeps the SDK and swaps out only the export path:

| Piece | File | What it does |
| --- | --- | --- |
| Simple processors | `BrowserTelemetryExtensions.cs` | Hand each finished span or log to an exporter right away, with no threads. |
| OTLP/JSON exporters | `OtlpJsonExporters.cs` | Convert spans, logs, and metrics to the [OTLP/JSON](https://opentelemetry.io/docs/specs/otlp/#json-protobuf-encoding) format and queue them in a buffer. |
| Buffer | `OtlpJsonBuffer.cs` | Holds the converted items, and caps their number in case the server can't be reached. |
| Shipper | `TelemetryShipper.cs` | Every 5 seconds, collects metrics and POSTs each signal to `/otlp/v1/...` asynchronously. |
| Tracing handler | `TracingHandler.cs` | A `DelegatingHandler` on the app's `HttpClient`. It creates an HTTP client span and adds the `traceparent` header using `DistributedContextPropagator`. |

The resource is built with `ResourceBuilder.CreateEmpty()`, because the default resource detectors use APIs that aren't available in the browser.

The demo page is `Pages/ClientTelemetry.razor`. It uses `InteractiveWebAssemblyRenderMode(prerender: false)` because its services exist only in the browser.

## Alternatives

- **Send directly from the browser to a collector.** The Aspire dashboard can accept browser telemetry directly if CORS is configured (`ASPIRE_DASHBOARD_CORS_ALLOWED_ORIGINS`). A server proxy avoids exposing the collector and its API key to the browser, and the app needs no CORS configuration.
- **JavaScript OpenTelemetry SDK.** `@opentelemetry/sdk-trace-web` can instrument the page (fetch, document load, user interactions). The .NET approach shown here keeps the Blazor component code in C# and uses the same APIs as the server.
- **Re-emit on the server.** The client could post simple DTOs to an API, and the server could log them or turn them into spans. That's simpler, but the telemetry then belongs to the server rather than to a real client service with its own spans.
