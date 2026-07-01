using Microsoft.Extensions.Caching.Memory;



namespace DotnetVibe.ApiService.WeatherMap;



public sealed class CachingWeatherProvider(

    IWeatherProvider inner,

    IMemoryCache cache,

    TimeSpan cacheDuration) : IWeatherProvider

{

    public async Task<LocationForecast> GetForecastAsync(

        double latitude,

        double longitude,

        ForecastFetchOptions? options = null,

        CancellationToken cancellationToken = default)

    {

        var cacheKey = CreateCacheKey(latitude, longitude);

        if (options?.BypassCache != true

            && cache.TryGetValue(cacheKey, out LocationForecast? cachedForecast)

            && cachedForecast is not null)

        {

            return cachedForecast;

        }



        var forecast = await inner.GetForecastAsync(latitude, longitude, options, cancellationToken);

        cache.Set(cacheKey, forecast, new MemoryCacheEntryOptions

        {

            AbsoluteExpirationRelativeToNow = cacheDuration,

            Size = 1

        });

        return forecast;

    }



    internal static string CreateCacheKey(double latitude, double longitude) =>

        $"weather-map:{Math.Round(latitude, 2):F2}:{Math.Round(longitude, 2):F2}";

}

