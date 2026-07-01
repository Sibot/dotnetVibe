namespace DotnetVibe.ApiService.WeatherMap;

public sealed record ForecastFetchOptions(bool BypassCache = false)
{
    public static ForecastFetchOptions Refresh { get; } = new(BypassCache: true);
}
