using System.Threading.RateLimiting;

using DotnetVibe.ApiService.WeatherMap;
using DotnetVibe.ApiService.WeatherMap.OpenMeteo;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DotnetVibe.ApiService;

public static class WeatherMapServiceExtensions
{
    public static IServiceCollection AddWeatherMapServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WeatherMapOptions>(configuration.GetSection("WeatherMap"));
        services.AddMemoryCache(options => options.SizeLimit = 1_000);

        var baseUrl = configuration["WeatherMap:OpenMeteoBaseUrl"] ?? "https://api.open-meteo.com/";
        var openMeteoBaseUri = OpenMeteoUrlValidator.ValidateAndCreate(baseUrl);

        services.AddHttpClient<OpenMeteoWeatherProvider>(client =>
            {
                client.BaseAddress = openMeteoBaseUri;
                client.Timeout = TimeSpan.FromSeconds(15);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AllowAutoRedirect = false
            });

        services.AddSingleton<IWeatherProvider>(serviceProvider =>
        {
            var inner = serviceProvider.GetRequiredService<OpenMeteoWeatherProvider>();
            var cache = serviceProvider.GetRequiredService<IMemoryCache>();
            var options = serviceProvider.GetRequiredService<IOptions<WeatherMapOptions>>().Value;
            return new CachingWeatherProvider(
                inner,
                cache,
                TimeSpan.FromMinutes(options.ForecastCacheMinutes));
        });

        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rateLimiterOptions.AddPolicy("forecast", httpContext =>
            {
                var options = httpContext.RequestServices.GetRequiredService<IOptions<WeatherMapOptions>>().Value;
                var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = options.ForecastRateLimitPermitsPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });

            rateLimiterOptions.AddPolicy("pinned-locations", httpContext =>
            {
                var userId = httpContext.User.FindFirst("sub")?.Value
                    ?? httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    userId,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });

            rateLimiterOptions.AddPolicy("authenticated-mutations", httpContext =>
            {
                var partitionKey = httpContext.User.FindFirst("sub")?.Value
                    ?? httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }
}
