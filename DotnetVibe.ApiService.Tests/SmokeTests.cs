using System.Net;

using DotnetVibe.ApiService.Tests.Integration;

namespace DotnetVibe.ApiService.Tests;

public sealed class SmokeTests(WeatherMapWebApplicationFactory factory) : IClassFixture<WeatherMapWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Api_boots_and_serves_root()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("API service is running", body, StringComparison.Ordinal);
    }
}
