using Azure.Messaging.ServiceBus;

namespace DotnetVibe.ApiService.Services;

public sealed class TemperatureQueueConsumer(
    ServiceBusClient serviceBusClient,
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<TemperatureQueueConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = configuration["TEMPERATURE_EVENTS_QUEUENAME"] ?? "temperature-events";
        await using var processor = serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
        });

        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ProcessErrorAsync;

        try
        {
            await processor.StartProcessingAsync(CancellationToken.None);

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
        }
        finally
        {
            logger.LogInformation("Stopping temperature queue consumer");
            await processor.StopProcessingAsync(CancellationToken.None);
        }
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var processor = scope.ServiceProvider.GetRequiredService<TemperatureMessageProcessor>();
        var processed = await processor.ProcessAsync(
            args.Message.MessageId,
            args.CancellationToken);

        if (processed)
        {
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Service Bus error from {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }
}
