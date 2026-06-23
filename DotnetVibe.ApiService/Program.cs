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
builder.Services.AddSignalR();
builder.Services.AddSingleton<WeatherForecastCacheService>();
builder.Services.AddScoped<WeatherForecastService>();
builder.Services.AddSingleton<TemperatureEventPublisher>();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
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

app.MapPost("/temperature/warm-up", async (TemperatureEventPublisher publisher, CancellationToken cancellationToken) =>
{
    await publisher.PublishWarmUpAsync(cancellationToken);
    return Results.Accepted();
})
.WithName("RequestTemperatureWarmUp");

app.MapHub<WeatherHub>("/hubs/weather");
app.MapDefaultEndpoints();

app.Run();
