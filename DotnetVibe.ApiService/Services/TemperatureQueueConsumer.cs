using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using DotnetVibe.ApiService.Data;
using DotnetVibe.ApiService.Hubs;
using DotnetVibe.ApiService.Models;

namespace DotnetVibe.ApiService.Services;

public sealed class TemperatureQueueConsumer(
    ServiceBusClient serviceBusClient,
    IServiceScopeFactory scopeFactory,
    IHubContext<WeatherHub> weatherHub,
    WeatherForecastCacheService weatherForecastCache,
    IConfiguration configuration,
    ILogger<TemperatureQueueConsumer> logger) : BackgroundService
{
    private const int TargetForecastId = 1;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = configuration["TEMPERATURE_EVENTS_QUEUENAME"] ?? "temperature-events";
        await using var processor = serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
        });

        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ProcessErrorAsync;

        await processor.StartProcessingAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        finally
        {
            await processor.StopProcessingAsync(CancellationToken.None);
        }
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var forecast = await db.WeatherForecasts
            .SingleOrDefaultAsync(forecast => forecast.Id == TargetForecastId, args.CancellationToken);

        if (forecast is not null)
        {
            forecast.TemperatureC += 1;
            await db.SaveChangesAsync(args.CancellationToken);
            await weatherForecastCache.InvalidateAsync(args.CancellationToken);

            var updatedForecast = new WeatherForecastDto(forecast.Date, forecast.TemperatureC);
            await weatherHub.Clients.All.SendAsync("ForecastUpdated", updatedForecast, args.CancellationToken);

            logger.LogInformation("Increased temperature for forecast {ForecastId} to {TemperatureC}C", forecast.Id, forecast.TemperatureC);
        }
        else
        {
            logger.LogWarning("Forecast {ForecastId} was not found", TargetForecastId);
        }

        await args.CompleteMessageAsync(args.Message, args.CancellationToken);
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Service Bus error from {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }
}
