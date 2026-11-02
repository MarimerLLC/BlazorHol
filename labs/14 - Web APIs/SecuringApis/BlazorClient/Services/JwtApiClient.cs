using System.Net.Http.Headers;

namespace BlazorClient.Services;

/// <summary>
/// Calls the ApiServer's JWT-protected endpoint. The ApiServer issues its own
/// tokens: POST a username and password to /jwt/token.
/// </summary>
public class JwtApiClient(HttpClient http)
{
    public async Task<TokenResult> GetTokenAsync(string username, string password)
    {
        var response = await http.PostAsJsonAsync("jwt/token", new { username, password });
        if (!response.IsSuccessStatusCode)
        {
            return new TokenResult(response.StatusCode, null, await response.Content.ReadAsStringAsync());
        }

        var result = await response.Content.ReadFromJsonAsync<JwtTokenResponse>();
        return new TokenResult(response.StatusCode, result!.AccessToken, null);
    }

    public async Task<ApiCallResult> GetForecastAsync(string? token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "jwt/weatherforecast");
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

    private record JwtTokenResponse(string AccessToken, string TokenType, int ExpiresIn);
}
