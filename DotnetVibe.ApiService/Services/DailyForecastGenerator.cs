namespace DotnetVibe.ApiService.Services;

public sealed class DailyForecastGenerator(
    IServiceScopeFactory scopeFactory,
    ILogger<DailyForecastGenerator> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Daily forecast generator started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunWithRetryAsync(stoppingToken);
            await Task.Delay(GetDelayUntilNextUtcMidnight(), stoppingToken);
        }
    }

    private async Task RunWithRetryAsync(CancellationToken stoppingToken)
    {
        const int maxAttempts = 5;

        for (var attempt = 1; attempt <= maxAttempts && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var dailyForecastService = scope.ServiceProvider.GetRequiredService<DailyForecastService>();
                await dailyForecastService.EnsureTodayForecastAsync(stoppingToken);
                return;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested && attempt < maxAttempts)
            {
                var delay = TimeSpan.FromSeconds(5 * attempt);
                logger.LogWarning(
                    ex,
                    "Failed to generate daily forecast (attempt {Attempt}/{MaxAttempts}); retrying in {DelaySeconds}s",
                    attempt,
                    maxAttempts,
                    delay.TotalSeconds);

                await Task.Delay(delay, stoppingToken);
            }
        }
    }

    private static TimeSpan GetDelayUntilNextUtcMidnight()
    {
        var now = DateTime.UtcNow;
        return now.Date.AddDays(1) - now;
    }
}
