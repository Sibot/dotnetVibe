using Microsoft.AspNetCore.SignalR.Client;

namespace DotnetVibe.Web.Services;

public sealed class WeatherHubClient : IAsyncDisposable
{
    private readonly Uri hubUri;
    private readonly SemaphoreSlim connectionLock = new(1, 1);
    private readonly ILogger<WeatherHubClient> logger;
    private HubConnection? hubConnection;

    public WeatherHubClient(
        IConfiguration configuration,
        IHostApplicationLifetime lifetime,
        ILogger<WeatherHubClient> logger)
    {
        hubUri = GetHubUri(configuration);
        this.logger = logger;
        lifetime.ApplicationStopping.Register(DisconnectForShutdown);
    }

    public event Func<WeatherForecast, Task>? ForecastUpdated;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        await connectionLock.WaitAsync(cancellationToken);
        try
        {
            hubConnection ??= BuildConnection();

            hubConnection.Remove("ForecastUpdated");
            hubConnection.On<WeatherForecast>("ForecastUpdated", async forecast =>
            {
                if (ForecastUpdated is not null)
                {
                    await ForecastUpdated.Invoke(forecast);
                }
            });

            if (hubConnection.State == HubConnectionState.Disconnected)
            {
                await hubConnection.StartAsync(cancellationToken);
                logger.LogInformation("Connected to weather hub at {HubUrl}", hubUri);
            }
        }
        finally
        {
            connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }

        connectionLock.Dispose();
    }

    private void DisconnectForShutdown()
    {
        try
        {
            if (hubConnection?.State is HubConnectionState.Connected or HubConnectionState.Reconnecting)
            {
                hubConnection.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Weather hub disconnect during shutdown");
        }
    }

    private HubConnection BuildConnection()
    {
        return new HubConnectionBuilder()
            .WithUrl(hubUri)
            .WithAutomaticReconnect()
            .Build();
    }

    private static Uri GetHubUri(IConfiguration configuration)
    {
        var baseUrl = configuration["services:apiservice:https:0"]
            ?? configuration["services:apiservice:http:0"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "ApiService endpoint was not found. Run the application through the AppHost so service references are injected.");
        }

        return new Uri($"{baseUrl.TrimEnd('/')}/hubs/weather");
    }
}
