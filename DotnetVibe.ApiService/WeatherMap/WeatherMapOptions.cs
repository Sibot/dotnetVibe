namespace DotnetVibe.ApiService.WeatherMap;

public sealed class WeatherMapOptions
{
    public string OpenMeteoBaseUrl { get; set; } = "https://api.open-meteo.com/";

    public int ForecastCacheMinutes { get; set; } = 10;

    public int ForecastRateLimitPermitsPerMinute { get; set; } = 30;
}
