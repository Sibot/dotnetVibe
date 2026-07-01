using DotnetVibe.ApiService.WeatherMap.Data;

using Microsoft.EntityFrameworkCore;

namespace DotnetVibe.ApiService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<WeatherForecast> WeatherForecasts => Set<WeatherForecast>();

    public DbSet<PinnedLocation> PinnedLocations => Set<PinnedLocation>();

    public DbSet<ProcessedTemperatureMessage> ProcessedTemperatureMessages => Set<ProcessedTemperatureMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherForecast>()
            .HasIndex(forecast => forecast.Date)
            .IsUnique();

        modelBuilder.Entity<WeatherForecast>().HasData(
            new WeatherForecast { Id = 1, Date = new DateOnly(2026, 6, 10), TemperatureC = 22 },
            new WeatherForecast { Id = 2, Date = new DateOnly(2026, 6, 11), TemperatureC = 18 },
            new WeatherForecast { Id = 3, Date = new DateOnly(2026, 6, 12), TemperatureC = 25 },
            new WeatherForecast { Id = 4, Date = new DateOnly(2026, 6, 13), TemperatureC = 12 },
            new WeatherForecast { Id = 5, Date = new DateOnly(2026, 6, 14), TemperatureC = 30 });

        modelBuilder.Entity<PinnedLocation>(entity =>
        {
            entity.HasIndex(location => new { location.UserId, location.SortOrder });
            entity.Property(location => location.Name).HasMaxLength(100);
            entity.Property(location => location.UserId).HasMaxLength(450);
        });

        modelBuilder.Entity<ProcessedTemperatureMessage>(entity =>
        {
            entity.HasKey(message => message.MessageId);
            entity.Property(message => message.MessageId).HasMaxLength(128);
        });
    }
}
