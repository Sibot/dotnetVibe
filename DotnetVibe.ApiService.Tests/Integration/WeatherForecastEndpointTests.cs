using System.Net;
using System.Net.Http.Json;

using DotnetVibe.ApiService.Tests.Integration;

namespace DotnetVibe.ApiService.Tests.Integration;

public sealed class WeatherForecastEndpointTests(WeatherMapWebApplicationFactory factory)
    : IClassFixture<WeatherMapWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetWeatherForecast_returns_seeded_forecasts()
    {
        var response = await _client.GetAsync("/weatherforecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecastResponse[]>();
        Assert.NotNull(forecasts);
        Assert.NotEmpty(forecasts!);
    }

    [Fact]
    public async Task PatchTemperature_without_auth_returns_401()
    {
        var response = await _client.PatchAsync(
            "/weatherforecast/2026-06-10/temperature?delta=1",
            null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PatchTemperature_with_invalid_delta_returns_bad_request_with_error_body()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", "admin-user");

        var response = await client.PatchAsync(
            "/weatherforecast/2026-06-10/temperature?delta=0",
            null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("error", body, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record WeatherForecastResponse(DateOnly Date, int TemperatureC);
}
