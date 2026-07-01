namespace DotnetVibe.ApiService.WeatherMap;

public interface IWeatherProvider
{
    Task<LocationForecast> GetForecastAsync(
        double latitude,
        double longitude,
        ForecastFetchOptions? options = null,
        CancellationToken cancellationToken = default);
}
