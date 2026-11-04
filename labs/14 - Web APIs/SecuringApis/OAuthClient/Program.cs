using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

// Calls the ApiServer's OAuth-protected endpoint.
// Access tokens come from the AuthServer using the OAuth 2.0 client credentials flow:
// the client authenticates with its own client id and secret and asks for scopes.

const string AuthServer = "https://localhost:7101";
const string ApiServer = "https://localhost:7201";

using var http = new HttpClient();

Console.WriteLine("=== 1. No token ===");
await CallWeatherApi(token: null);

Console.WriteLine("=== 2. weather-client requests weather.read ===");
var weatherToken = await GetToken("weather-client", "weather-client-secret", "weather.read");
await CallWeatherApi(weatherToken);

Console.WriteLine("=== 3. alerts-client requests weather.read (not permitted for this client) ===");
await GetToken("alerts-client", "alerts-client-secret", "weather.read");

Console.WriteLine("=== 4. alerts-client requests alerts.read (valid token, wrong scope for this API call) ===");
var alertsToken = await GetToken("alerts-client", "alerts-client-secret", "alerts.read");
await CallWeatherApi(alertsToken);

async Task<string?> GetToken(string clientId, string clientSecret, string scope)
{
    // The token request is a form post, as defined by the OAuth 2.0 spec
    var response = await http.PostAsync($"{AuthServer}/connect/token", new FormUrlEncodedContent(
        new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["scope"] = scope
        }));

    Console.WriteLine($"POST /connect/token as {clientId}: {(int)response.StatusCode} {response.StatusCode}");
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        Console.WriteLine();
        return null;
    }

    var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
    Console.WriteLine("Token payload:");
    Console.WriteLine(DecodePayload(result!.AccessToken));
    return result.AccessToken;
}

async Task CallWeatherApi(string? token)
{
    using var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiServer}/oauth/weatherforecast");
    if (token is not null)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    var response = await http.SendAsync(request);
    Console.WriteLine($"GET /oauth/weatherforecast: {(int)response.StatusCode} {response.StatusCode}");
    if (response.IsSuccessStatusCode)
    {
        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();
        foreach (var forecast in forecasts!)
        {
            Console.WriteLine($"  {forecast.Date}  {forecast.TemperatureC,3}C  {forecast.Summary}");
        }
    }
    Console.WriteLine();
}

// A JWT is three base64url segments: header.payload.signature.
// The payload is readable by anyone; the signature is what makes it trustworthy.
static string DecodePayload(string jwt)
{
    var payload = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
    payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
    var json = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
    return JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true });
}

// OAuth token responses use snake_case property names
record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);
