using Microsoft.EntityFrameworkCore;
using DotnetVibe.ApiService.Data;
using DotnetVibe.ApiService.Hubs;
using DotnetVibe.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<AppDbContext>("dotnetvibedb");
builder.AddAzureServiceBusClient("temperature-events");
builder.AddRedisDistributedCache("cache");
builder.Services.AddHostedService<TemperatureQueueConsumer>();
builder.Services.AddHostedService<DailyForecastGenerator>();
builder.Services.AddScoped<DailyForecastService>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<WeatherForecastCacheService>();
builder.Services.AddScoped<WeatherForecastService>();
builder.Services.AddSingleton<TemperatureEventPublisher>();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var dailyForecastService = scope.ServiceProvider.GetRequiredService<DailyForecastService>();
    await dailyForecastService.EnsureTodayForecastAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

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
    if (delta is 0)
    {
        return Results.BadRequest();
    }

    var updated = await weatherForecastService.AdjustTemperatureAsync(date, delta, cancellationToken);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
})
.WithName("AdjustForecastTemperature");

app.MapPost("/temperature/warm-up", async (TemperatureEventPublisher publisher, CancellationToken cancellationToken) =>
{
    await publisher.PublishWarmUpAsync(cancellationToken);
    return Results.Accepted();
})
.WithName("RequestTemperatureWarmUp");

app.MapHub<WeatherHub>("/hubs/weather");
app.MapDefaultEndpoints();

app.Run();
