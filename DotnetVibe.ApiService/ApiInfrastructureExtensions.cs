using DotnetVibe.ApiService.Data;
using DotnetVibe.ApiService.Services;

namespace DotnetVibe.ApiService;

public static class ApiInfrastructureExtensions
{
    public static IHostApplicationBuilder AddApiInfrastructure(this IHostApplicationBuilder builder)
    {
        ApiInfrastructureConfiguration.RequireForDeployment(builder.Environment, builder.Configuration);

        if (!ApiInfrastructureConfiguration.IsConfigured(builder.Configuration))
        {
            return builder;
        }

        builder.AddSqlServerDbContext<AppDbContext>(ApiInfrastructureConfiguration.DatabaseConnectionName);
        builder.AddAzureServiceBusClient(ApiInfrastructureConfiguration.ServiceBusConnectionName);
        builder.AddRedisDistributedCache(ApiInfrastructureConfiguration.CacheConnectionName);
        builder.Services.AddHostedService<TemperatureQueueConsumer>();
        builder.Services.AddHostedService<DailyForecastGenerator>();
        builder.Services.AddSingleton<TemperatureEventPublisher>();

        return builder;
    }
}
