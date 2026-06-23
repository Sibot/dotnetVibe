using Microsoft.EntityFrameworkCore;
using DotnetVibe.ApiService.Data;
using DotnetVibe.ApiService.Models;

namespace DotnetVibe.ApiService.Services;

public sealed class WeatherForecastService(
    AppDbContext db,
    WeatherForecastCacheService cache,
    ILogger<WeatherForecastService> logger)
{
    public async Task<WeatherForecastDto[]> GetForecastsAsync(CancellationToken cancellationToken = default)
    {
        var cachedForecasts = await cache.GetAsync(cancellationToken);
        if (cachedForecasts is not null)
        {
            logger.LogInformation(
                "Serving {ForecastCount} weather forecasts from Redis cache (key: {CacheKey})",
                cachedForecasts.Length,
                WeatherForecastCacheService.CacheKey);
            return cachedForecasts;
        }

        logger.LogInformation(
            "Cache miss for key {CacheKey}; loading weather forecasts from database",
            WeatherForecastCacheService.CacheKey);

        var forecasts = await db.WeatherForecasts
            .OrderBy(forecast => forecast.Date)
            .Select(forecast => new WeatherForecastDto(forecast.Date, forecast.TemperatureC, forecast.Summary))
            .ToArrayAsync(cancellationToken);

        await cache.SetAsync(forecasts, cancellationToken);

        logger.LogInformation(
            "Loaded {ForecastCount} weather forecasts from database and stored in Redis cache",
            forecasts.Length);

        return forecasts;
    }
}
