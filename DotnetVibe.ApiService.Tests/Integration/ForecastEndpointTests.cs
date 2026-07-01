using System.Net;
using System.Net.Http.Json;

using DotnetVibe.ApiService.WeatherMap;

namespace DotnetVibe.ApiService.Tests.Integration;

public sealed class ForecastEndpointTests(WeatherMapWebApplicationFactory factory) : IClassFixture<WeatherMapWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetForecast_returns_current_and_daily_forecast()
    {
        ConfigureSuccessfulForecast();

        var response = await _client.GetAsync("/weather-map/forecast?latitude=52.52&longitude=13.41");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LocationForecastResponse>();
        Assert.NotNull(body);
        Assert.Equal(22.5, body.Current.TemperatureCelsius);
        Assert.Equal(2, body.Daily.Count);
        Assert.Equal("Clear sky", body.Daily[0].Summary);
    }

    [Fact]
    public async Task GetForecast_returns_400_for_invalid_coordinates()
    {
        var response = await _client.GetAsync("/weather-map/forecast?latitude=120&longitude=13.41");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetForecast_does_not_require_authentication()
    {
        ConfigureSuccessfulForecast();

        var response = await _client.GetAsync("/weather-map/forecast?latitude=52.52&longitude=13.41");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetForecast_returns_generic_error_when_provider_fails()
    {
        factory.FakeWeatherProvider.ShouldThrowWeatherProviderException = true;

        try
        {
            var response = await _client.GetAsync("/weather-map/forecast?latitude=52.52&longitude=13.41");
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
            Assert.DoesNotContain("Open-Meteo", body, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Weather service unavailable", body, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            factory.FakeWeatherProvider.ShouldThrowWeatherProviderException = false;
        }
    }

    [Fact]
    public async Task GetForecast_accepts_refresh_query_parameter()
    {
        ConfigureSuccessfulForecast();

        var response = await _client.GetAsync(
            "/weather-map/forecast?latitude=52.52&longitude=13.41&refresh=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetForecast_includes_security_headers()
    {
        ConfigureSuccessfulForecast();

        var response = await _client.GetAsync("/weather-map/forecast?latitude=52.52&longitude=13.41");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
    }

    private void ConfigureSuccessfulForecast()
    {
        factory.FakeWeatherProvider.ShouldThrowWeatherProviderException = false;
        factory.FakeWeatherProvider.NextForecast = SampleForecast();
    }

    private static LocationForecast SampleForecast() =>
        new(
            new GeoPoint(52.52, 13.41),
            new CurrentConditions(
                new DateTimeOffset(2026, 6, 30, 12, 0, 0, TimeSpan.Zero),
                22.5,
                "Clear sky",
                10.2),
            [
                new DailyForecast(new DateOnly(2026, 6, 30), 25, 15, "Clear sky"),
                new DailyForecast(new DateOnly(2026, 7, 1), 26, 16, "Mainly clear")
            ]);

    private sealed record LocationForecastResponse(
        GeoPointResponse Location,
        CurrentConditionsResponse Current,
        IReadOnlyList<DailyForecastResponse> Daily);

    private sealed record GeoPointResponse(double Latitude, double Longitude);

    private sealed record CurrentConditionsResponse(
        DateTimeOffset ObservedAt,
        double TemperatureCelsius,
        string Summary,
        double WindSpeedKph);

    private sealed record DailyForecastResponse(
        DateOnly Date,
        double TemperatureMaxCelsius,
        double TemperatureMinCelsius,
        string Summary);
}
