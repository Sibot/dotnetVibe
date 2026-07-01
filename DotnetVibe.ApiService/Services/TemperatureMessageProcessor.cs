using DotnetVibe.ApiService.Data;
using DotnetVibe.ApiService.Hubs;
using DotnetVibe.ApiService.Models;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DotnetVibe.ApiService.Services;

public sealed class TemperatureMessageProcessor(
    AppDbContext db,
    IHubContext<WeatherHub> weatherHub,
    WeatherForecastCacheService weatherForecastCache,
    ILogger<TemperatureMessageProcessor> logger)
{
    public async Task<bool> ProcessAsync(string messageId, CancellationToken cancellationToken = default)
    {
        if (await db.ProcessedTemperatureMessages.AnyAsync(
                message => message.MessageId == messageId,
                cancellationToken))
        {
            logger.LogDebug("Skipping duplicate temperature message {MessageId}", messageId);
            return true;
        }

        var forecast = await db.WeatherForecasts
            .SingleOrDefaultAsync(
                entry => entry.Id == DemoForecastIds.WarmUpTarget,
                cancellationToken);

        if (forecast is null)
        {
            logger.LogWarning("Forecast {ForecastId} was not found", DemoForecastIds.WarmUpTarget);
            return true;
        }

        forecast.TemperatureC += 1;
        db.ProcessedTemperatureMessages.Add(new ProcessedTemperatureMessage
        {
            MessageId = messageId,
            ProcessedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
        await weatherForecastCache.InvalidateAsync(cancellationToken);

        var updatedForecast = new WeatherForecastDto(forecast.Date, forecast.TemperatureC);
        await weatherHub.Clients.All.SendAsync("ForecastUpdated", updatedForecast, cancellationToken);

        logger.LogInformation(
            "Increased temperature for forecast {ForecastId} to {TemperatureC}C",
            forecast.Id,
            forecast.TemperatureC);
        return true;
    }
}
