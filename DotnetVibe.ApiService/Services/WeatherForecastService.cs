using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using DotnetVibe.ApiService.Data;
using DotnetVibe.ApiService.Hubs;
using DotnetVibe.ApiService.Models;

namespace DotnetVibe.ApiService.Services;

public sealed class WeatherForecastService(
    AppDbContext db,
    WeatherForecastCacheService cache,
    IHubContext<WeatherHub> weatherHub,
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
            .Select(forecast => new WeatherForecastDto(forecast.Date, forecast.TemperatureC))
            .ToArrayAsync(cancellationToken);

        await cache.SetAsync(forecasts, cancellationToken);

        logger.LogInformation(
            "Loaded {ForecastCount} weather forecasts from database and stored in Redis cache",
            forecasts.Length);

        return forecasts;
    }

    public async Task<WeatherForecastDto?> AdjustTemperatureAsync(
        DateOnly date,
        int delta,
        CancellationToken cancellationToken = default)
    {
        var forecast = await db.WeatherForecasts
            .SingleOrDefaultAsync(entry => entry.Date == date, cancellationToken);

        if (forecast is null)
        {
            return null;
        }

        forecast.TemperatureC += delta;
        await db.SaveChangesAsync(cancellationToken);
        await cache.InvalidateAsync(cancellationToken);

        var dto = new WeatherForecastDto(forecast.Date, forecast.TemperatureC);
        await weatherHub.Clients.All.SendAsync("ForecastUpdated", dto, cancellationToken);

        logger.LogInformation(
            "Adjusted temperature for {Date} by {Delta}C to {TemperatureC}C",
            date,
            delta,
            forecast.TemperatureC);

        return dto;
    }
}
