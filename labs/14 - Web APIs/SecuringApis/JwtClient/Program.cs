using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

// Calls the ApiServer's JWT-protected endpoint.
// The API issues its own tokens: POST a username and password to /jwt/token.

const string ApiServer = "https://localhost:7201";

using var http = new HttpClient { BaseAddress = new Uri(ApiServer) };

Console.WriteLine("=== 1. No token ===");
await CallWeatherApi(token: null);

Console.WriteLine("=== 2. Wrong password ===");
await GetToken("alice", "wrong-password");

Console.WriteLine("=== 3. alice (has the weather.read scope) ===");
var aliceToken = await GetToken("alice", "alice-password");
await CallWeatherApi(aliceToken);

Console.WriteLine("=== 4. bob (authenticated, but no weather.read scope) ===");
var bobToken = await GetToken("bob", "bob-password");
await CallWeatherApi(bobToken);

async Task<string?> GetToken(string username, string password)
{
    var response = await http.PostAsJsonAsync("/jwt/token", new { username, password });
    Console.WriteLine($"POST /jwt/token as {username}: {(int)response.StatusCode} {response.StatusCode}");
    if (!response.IsSuccessStatusCode)
    {
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
    using var request = new HttpRequestMessage(HttpMethod.Get, "/jwt/weatherforecast");
    if (token is not null)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    var response = await http.SendAsync(request);
    Console.WriteLine($"GET /jwt/weatherforecast: {(int)response.StatusCode} {response.StatusCode}");
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

record TokenResponse(string AccessToken, string TokenType, int ExpiresIn);

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);
