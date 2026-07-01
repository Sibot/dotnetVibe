using DotnetVibe.ApiService.Data;
using DotnetVibe.ApiService.Hubs;
using DotnetVibe.ApiService.Services;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DotnetVibe.ApiService.Tests.Services;

public sealed class TemperatureMessageProcessorTests
{
    [Fact]
    public async Task ProcessAsync_increments_temperature_once_per_message_id()
    {
        await using var db = CreateDatabase();
        var forecast = new WeatherForecast { Id = DemoForecastIds.WarmUpTarget, Date = new DateOnly(2026, 6, 10), TemperatureC = 20 };
        db.WeatherForecasts.Add(forecast);
        await db.SaveChangesAsync();

        var processor = CreateProcessor(db);
        var first = await processor.ProcessAsync("message-1");
        var second = await processor.ProcessAsync("message-1");

        Assert.True(first);
        Assert.True(second);
        Assert.Equal(21, (await db.WeatherForecasts.SingleAsync()).TemperatureC);
        Assert.Single(db.ProcessedTemperatureMessages);
    }

    [Fact]
    public async Task ProcessAsync_processes_distinct_message_ids_separately()
    {
        await using var db = CreateDatabase();
        db.WeatherForecasts.Add(new WeatherForecast
        {
            Id = DemoForecastIds.WarmUpTarget,
            Date = new DateOnly(2026, 6, 10),
            TemperatureC = 10
        });
        await db.SaveChangesAsync();

        var processor = CreateProcessor(db);
        await processor.ProcessAsync("message-1");
        await processor.ProcessAsync("message-2");

        Assert.Equal(12, (await db.WeatherForecasts.SingleAsync()).TemperatureC);
        Assert.Equal(2, await db.ProcessedTemperatureMessages.CountAsync());
    }

    private static TemperatureMessageProcessor CreateProcessor(AppDbContext db)
    {
        var hubContext = new FakeHubContext();
        var cache = new WeatherForecastCacheService(new MemoryDistributedCache(
            new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions())),
            NullLogger<WeatherForecastCacheService>.Instance);
        return new TemperatureMessageProcessor(db, hubContext, cache, NullLogger<TemperatureMessageProcessor>.Instance);
    }

    private static AppDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private sealed class FakeHubContext : IHubContext<WeatherHub>
    {
        public IHubClients Clients { get; } = new FakeHubClients();
        public IGroupManager Groups { get; } = null!;
    }

    private sealed class FakeHubClients : IHubClients
    {
        public IClientProxy All { get; } = new FakeClientProxy();
        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => All;
        public IClientProxy Client(string connectionId) => All;
        public IClientProxy Clients(IReadOnlyList<string> connectionIds) => All;
        public IClientProxy Group(string groupName) => All;
        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => All;
        public IClientProxy Groups(IReadOnlyList<string> groupNames) => All;
        public IClientProxy User(string userId) => All;
        public IClientProxy Users(IReadOnlyList<string> userIds) => All;
    }

    private sealed class FakeClientProxy : IClientProxy
    {
        public Task SendCoreAsync(string methodName, object?[] args, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
