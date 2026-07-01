using System.Text.Json;

using DotnetVibe.ApiService.Models;

using Microsoft.Extensions.Caching.Distributed;

namespace DotnetVibe.ApiService.Services;

public sealed class WeatherForecastCacheService(
    IDistributedCache cache,
    ILogger<WeatherForecastCacheService> logger)
{
    public const string CacheKey = "weather:forecasts";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<WeatherForecastDto[]?> GetAsync(CancellationToken cancellationToken = default)
    {
        var cachedBytes = await cache.GetAsync(CacheKey, cancellationToken);
        if (cachedBytes is null or { Length: 0 })
        {
            return null;
        }

        return JsonSerializer.Deserialize<WeatherForecastDto[]>(cachedBytes, JsonOptions);
    }

    public async Task SetAsync(WeatherForecastDto[] forecasts, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(forecasts, JsonOptions);
        await cache.SetAsync(
            CacheKey,
            bytes,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheTtl
            },
            cancellationToken);
        logger.LogDebug("Wrote {ForecastCount} weather forecasts to Redis cache (key: {CacheKey})", forecasts.Length, CacheKey);
    }

    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(CacheKey, cancellationToken);
        logger.LogInformation("Invalidated Redis cache (key: {CacheKey}) after weather data update", CacheKey);
    }
}
