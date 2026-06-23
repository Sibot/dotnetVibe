using Azure.Messaging.ServiceBus;

namespace DotnetVibe.ApiService.Services;

public sealed class TemperatureEventPublisher(
    ServiceBusClient serviceBusClient,
    IConfiguration configuration,
    ILogger<TemperatureEventPublisher> logger)
{
    public async Task PublishWarmUpAsync(CancellationToken cancellationToken = default)
    {
        var queueName = configuration["TEMPERATURE_EVENTS_QUEUENAME"] ?? "temperature-events";
        await using var sender = serviceBusClient.CreateSender(queueName);
        var message = new ServiceBusMessage("warm-up-forecast-1");
        await sender.SendMessageAsync(message, cancellationToken);
        logger.LogInformation("Published warm-up event to queue {QueueName}", queueName);
    }
}
