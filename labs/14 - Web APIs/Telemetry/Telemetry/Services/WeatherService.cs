using Telemetry.Client.Models;

namespace Telemetry.Services;

public class WeatherService(ILogger<WeatherService> logger)
{
    private static readonly string[] Summaries =
        ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    public async Task<WeatherForecast[]> GetForecastsAsync(int days, string caller)
    {
        using var activity = ServerDiagnostics.ActivitySource.StartActivity("Generate forecasts");
        activity?.SetTag("weather.days", days);
        activity?.SetTag("weather.caller", caller);

        // Simulate a slow data source so the span has a visible duration
        await Task.Delay(Random.Shared.Next(100, 400));
        activity?.AddEvent(new("Data source queried"));

        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var forecasts = Enumerable.Range(1, days).Select(index => new WeatherForecast
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToArray();

        ServerDiagnostics.ForecastsGenerated.Add(forecasts.Length, new KeyValuePair<string, object?>("weather.caller", caller));
        logger.LogInformation("Generated {ForecastCount} forecasts for {Caller}", forecasts.Length, caller);

        return forecasts;
    }
}
