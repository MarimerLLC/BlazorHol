using System.Net;
using System.Text;
using System.Text.Json;

namespace BlazorClient.Services;

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);

/// <summary>
/// The outcome of a token request: the access token if one was issued,
/// otherwise the error returned by the token endpoint.
/// </summary>
public record TokenResult(HttpStatusCode StatusCode, string? AccessToken, string? Error)
{
    public bool Succeeded => AccessToken is not null;

    public string Status => $"{(int)StatusCode} {StatusCode}";

    // A JWT is three base64url segments: header.payload.signature.
    // The payload is readable by anyone; the signature is what makes it trustworthy.
    public string? Payload
    {
        get
        {
            if (AccessToken is null)
            {
                return null;
            }
            var payload = AccessToken.Split('.')[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var json = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
            return JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}

/// <summary>
/// The outcome of a call to a weather forecast endpoint.
/// </summary>
public record ApiCallResult(HttpStatusCode StatusCode, WeatherForecast[]? Forecasts)
{
    public string Status => $"{(int)StatusCode} {StatusCode}";
}
