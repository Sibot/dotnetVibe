using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using DotnetVibe.ApiService.Data;
using DotnetVibe.ApiService.Hubs;
using DotnetVibe.ApiService.Models;

namespace DotnetVibe.ApiService.Services;

public sealed class DailyForecastService(
    AppDbContext db,
    IHubContext<WeatherHub> weatherHub,
    WeatherForecastCacheService weatherForecastCache,
    ILogger<DailyForecastService> logger)
{
    public async Task EnsureTodayForecastAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (await db.WeatherForecasts.AnyAsync(forecast => forecast.Date == today, cancellationToken))
        {
            logger.LogInformation("Forecast for {Date} already exists; skipping daily generation", today);
            await weatherForecastCache.InvalidateAsync(cancellationToken);
            return;
        }

        var temperatureC = Random.Shared.Next(-10, 36);
        var forecast = new WeatherForecast
        {
            Date = today,
            TemperatureC = temperatureC,
        };

        db.WeatherForecasts.Add(forecast);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueDateViolation(ex))
        {
            logger.LogInformation("Forecast for {Date} was created by another instance; skipping", today);
            await weatherForecastCache.InvalidateAsync(cancellationToken);
            return;
        }

        await weatherForecastCache.InvalidateAsync(cancellationToken);

        var dto = new WeatherForecastDto(forecast.Date, forecast.TemperatureC);
        await weatherHub.Clients.All.SendAsync("ForecastUpdated", dto, cancellationToken);

        logger.LogInformation(
            "Generated daily forecast for {Date} with temperature {TemperatureC}C ({Summary})",
            forecast.Date,
            forecast.TemperatureC,
            dto.Summary);
    }

    private static bool IsUniqueDateViolation(DbUpdateException exception) =>
        exception.InnerException?.Message.Contains("IX_WeatherForecasts_Date", StringComparison.OrdinalIgnoreCase) == true
        || exception.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true;
}
