using Microsoft.EntityFrameworkCore;

namespace DotnetVibe.ApiService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<WeatherForecast> WeatherForecasts => Set<WeatherForecast>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherForecast>().HasData(
            new WeatherForecast { Id = 1, Date = new DateOnly(2026, 6, 10), TemperatureC = 22, Summary = "Mild" },
            new WeatherForecast { Id = 2, Date = new DateOnly(2026, 6, 11), TemperatureC = 18, Summary = "Cool" },
            new WeatherForecast { Id = 3, Date = new DateOnly(2026, 6, 12), TemperatureC = 25, Summary = "Warm" },
            new WeatherForecast { Id = 4, Date = new DateOnly(2026, 6, 13), TemperatureC = 12, Summary = "Chilly" },
            new WeatherForecast { Id = 5, Date = new DateOnly(2026, 6, 14), TemperatureC = 30, Summary = "Hot" });
    }
}
