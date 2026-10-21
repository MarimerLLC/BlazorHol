var builder = DistributedApplication.CreateBuilder(args);

// The Blazor server app. Its OTLP exporter is switched to HTTP/protobuf so the
// endpoint Aspire injects (OTEL_EXPORTER_OTLP_ENDPOINT) is the dashboard's
// OTLP/HTTP listener. The server's /otlp proxy forwards browser telemetry to
// that same endpoint.
builder.AddProject<Projects.Telemetry>("blazor-server")
    .WithOtlpExporter(OtlpProtocol.HttpProtobuf)
    .WithExternalHttpEndpoints();

builder.Build().Run();
