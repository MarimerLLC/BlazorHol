using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace BlazorClient.Services;

/// <summary>
/// Calls the ApiServer's OAuth-protected endpoint. Access tokens come from the
/// AuthServer using the OAuth 2.0 client credentials flow.
/// </summary>
/// <remarks>
/// The client secrets are read from this app's server-side configuration. This
/// code runs on the server (Interactive Server rendering), so the secrets are
/// never sent to the browser. Never put a client secret in WebAssembly code.
/// </remarks>
public class OAuthApiClient(HttpClient http, IConfiguration configuration)
{
    public static readonly string[] ClientIds = ["weather-client", "alerts-client"];

    public static readonly string[] Scopes = ["weather.read", "alerts.read"];

    public async Task<TokenResult> GetTokenAsync(string clientId, string scope)
    {
        var tokenEndpoint = new Uri(new Uri(configuration["AuthServer"]!), "connect/token");
        var clientSecret = configuration[$"OAuthClients:{clientId}"]
            ?? throw new InvalidOperationException($"No client secret is configured for {clientId}.");

        // The token request is a form post, as defined by the OAuth 2.0 spec
        var response = await http.PostAsync(tokenEndpoint, new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["scope"] = scope
            }));

        if (!response.IsSuccessStatusCode)
        {
            return new TokenResult(response.StatusCode, null, await response.Content.ReadAsStringAsync());
        }

        var result = await response.Content.ReadFromJsonAsync<OAuthTokenResponse>();
        return new TokenResult(response.StatusCode, result!.AccessToken, null);
    }

    public async Task<ApiCallResult> GetForecastAsync(string? token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "oauth/weatherforecast");
        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await http.SendAsync(request);
        var forecasts = response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<WeatherForecast[]>()
            : null;
        return new ApiCallResult(response.StatusCode, forecasts);
    }

    // OAuth token responses use snake_case property names
    private record OAuthTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("token_type")] string TokenType,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
