using System.Net;

using DotnetVibe.ApiService.WeatherMap;
using DotnetVibe.ApiService.WeatherMap.OpenMeteo;

namespace DotnetVibe.ApiService.Tests.WeatherMap;

public sealed class OpenMeteoWeatherProviderTests
{
    [Fact]
    public async Task GetForecastAsync_maps_provider_response_to_location_forecast()
    {
        var fixtureJson = await File.ReadAllTextAsync("Fixtures/open-meteo-sample.json");
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(fixtureJson)
        });
        var provider = new OpenMeteoWeatherProvider(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.open-meteo.com/")
        });

        var forecast = await provider.GetForecastAsync(52.52, 13.41);

        Assert.Equal(52.52, forecast.Location.Latitude, precision: 2);
        Assert.Equal(13.42, forecast.Location.Longitude, precision: 2);
        Assert.Equal(22.5, forecast.Current.TemperatureCelsius);
        Assert.Equal("Clear sky", forecast.Current.Summary);
        Assert.Equal(10.2, forecast.Current.WindSpeedKph);
        Assert.Equal(3, forecast.Daily.Count);
        Assert.Equal(new DateOnly(2026, 6, 30), forecast.Daily[0].Date);
        Assert.Equal(25.0, forecast.Daily[0].TemperatureMaxCelsius);
        Assert.Equal(15.0, forecast.Daily[0].TemperatureMinCelsius);
        Assert.Equal("Clear sky", forecast.Daily[0].Summary);
        Assert.Equal("Slight rain", forecast.Daily[2].Summary);
    }

    [Fact]
    public async Task GetForecastAsync_throws_when_response_json_is_invalid()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{not-json")
        });
        var provider = new OpenMeteoWeatherProvider(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.open-meteo.com/")
        });

        await Assert.ThrowsAsync<WeatherProviderException>(
            () => provider.GetForecastAsync(52.52, 13.41));
    }

    [Fact]
    public async Task GetForecastAsync_throws_when_provider_returns_error()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var provider = new OpenMeteoWeatherProvider(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.open-meteo.com/")
        });

        await Assert.ThrowsAsync<WeatherProviderException>(
            () => provider.GetForecastAsync(52.52, 13.41));
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(responder(request));
    }
}
