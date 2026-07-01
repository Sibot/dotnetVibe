using DotnetVibe.ApiService.WeatherMap;

using Microsoft.Extensions.Caching.Memory;

namespace DotnetVibe.ApiService.Tests.WeatherMap;

public sealed class CachingWeatherProviderTests
{
    [Fact]
    public async Task GetForecastAsync_uses_cache_for_same_coordinates()
    {
        var inner = new CountingWeatherProvider(SampleForecast());
        var cache = new MemoryCache(new MemoryCacheOptions());
        var provider = new CachingWeatherProvider(inner, cache, TimeSpan.FromMinutes(10));

        await provider.GetForecastAsync(52.52, 13.41);
        await provider.GetForecastAsync(52.52, 13.41);

        Assert.Equal(1, inner.CallCount);
    }

    [Fact]
    public async Task GetForecastAsync_calls_inner_again_for_different_coordinates()
    {
        var inner = new CountingWeatherProvider(SampleForecast());
        var cache = new MemoryCache(new MemoryCacheOptions());
        var provider = new CachingWeatherProvider(inner, cache, TimeSpan.FromMinutes(10));

        await provider.GetForecastAsync(52.52, 13.41);
        await provider.GetForecastAsync(48.13, 11.58);

        Assert.Equal(2, inner.CallCount);
    }

    [Fact]
    public async Task GetForecastAsync_bypasses_cache_when_refresh_requested()
    {
        var inner = new CountingWeatherProvider(SampleForecast());
        var cache = new MemoryCache(new MemoryCacheOptions());
        var provider = new CachingWeatherProvider(inner, cache, TimeSpan.FromMinutes(10));

        await provider.GetForecastAsync(52.52, 13.41);
        await provider.GetForecastAsync(52.52, 13.41, ForecastFetchOptions.Refresh);

        Assert.Equal(2, inner.CallCount);
    }

    private static LocationForecast SampleForecast() =>
        new(
            new GeoPoint(52.52, 13.41),
            new CurrentConditions(DateTimeOffset.UtcNow, 20, "Clear sky", 5),
            [new DailyForecast(DateOnly.FromDateTime(DateTime.UtcNow), 22, 12, "Clear sky")]);

    private sealed class CountingWeatherProvider(LocationForecast forecast) : IWeatherProvider
    {
        public int CallCount { get; private set; }

        public Task<LocationForecast> GetForecastAsync(
            double latitude,
            double longitude,
            ForecastFetchOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(forecast);
        }
    }
}
