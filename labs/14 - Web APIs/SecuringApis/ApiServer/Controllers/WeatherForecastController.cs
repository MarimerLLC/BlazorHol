using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiServer.Controllers;

[ApiController]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    // Requires a JWT issued by this API's own token endpoint
    [HttpGet("jwt/weatherforecast")]
    [Authorize(Policy = Policies.JwtWeatherRead)]
    public IEnumerable<WeatherForecast> GetWithJwt() => CreateForecast();

    // Requires an OAuth access token issued by the AuthServer
    [HttpGet("oauth/weatherforecast")]
    [Authorize(Policy = Policies.OAuthWeatherRead)]
    public IEnumerable<WeatherForecast> GetWithOAuth() => CreateForecast();

    private static WeatherForecast[] CreateForecast()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
