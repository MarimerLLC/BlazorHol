namespace ApiServer;

public static class AuthSchemes
{
    public const string Jwt = "Jwt";
    public const string OAuth = "OAuth";
}

public static class Policies
{
    public const string JwtWeatherRead = "JwtWeatherRead";
    public const string OAuthWeatherRead = "OAuthWeatherRead";
}
