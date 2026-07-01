using DotnetVibe.ApiService;
using DotnetVibe.ApiService.Data;
using DotnetVibe.ApiService.Hubs;
using DotnetVibe.ApiService.Security;
using DotnetVibe.ApiService.Services;
using DotnetVibe.ApiService.WeatherMap;
using DotnetVibe.Auth;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddApiAuthentication(builder.Configuration, builder.Environment);
builder.AddApiInfrastructure();

builder.Services.AddScoped<DailyForecastService>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<WeatherForecastCacheService>();
builder.Services.AddScoped<WeatherForecastService>();
builder.Services.AddScoped<TemperatureMessageProcessor>();
builder.Services.AddScoped<PinnedLocationService>();
builder.Services.AddWeatherMapServices(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

// Development-only migrations; production/staging must run migrations in deployment pipeline.
if (app.Environment.IsDevelopment() && ApiInfrastructureConfiguration.IsConfigured(app.Configuration))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var dailyForecastService = scope.ServiceProvider.GetRequiredService<DailyForecastService>();
    await dailyForecastService.EnsureTodayForecastAsync();
}

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseSecurityHeaders(new SecurityHeadersOptions
{
    ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'"
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");

app.MapGet("/weatherforecast", async (WeatherForecastService weatherForecastService, CancellationToken cancellationToken) =>
    await weatherForecastService.GetForecastsAsync(cancellationToken))
.WithName("GetWeatherForecast");

app.MapPatch("/weatherforecast/{date}/temperature", async (
    DateOnly date,
    int delta,
    WeatherForecastService weatherForecastService,
    CancellationToken cancellationToken) =>
{
    try
    {
        TemperatureDeltaValidator.Validate(delta);
    }
    catch (InvalidTemperatureDeltaException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }

    var updated = await weatherForecastService.AdjustTemperatureAsync(date, delta, cancellationToken);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
})
.RequireAuthorization(AuthPolicies.AdjustTemperature)
.RequireRateLimiting("authenticated-mutations")
.WithName("AdjustForecastTemperature");

if (ApiInfrastructureConfiguration.IsConfigured(app.Configuration))
{
    app.MapPost("/temperature/warm-up", async (TemperatureEventPublisher publisher, CancellationToken cancellationToken) =>
    {
        await publisher.PublishWarmUpAsync(cancellationToken);
        return Results.Accepted();
    })
    .RequireAuthorization(AuthPolicies.WarmUp)
    .RequireRateLimiting("authenticated-mutations")
    .WithName("RequestTemperatureWarmUp");
}

app.MapHub<WeatherHub>("/hubs/weather");
app.MapWeatherMapEndpoints();
app.MapDefaultEndpoints();

app.Run();

public partial class Program;
