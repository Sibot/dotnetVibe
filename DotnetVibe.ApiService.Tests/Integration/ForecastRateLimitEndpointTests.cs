using System.Net;



using DotnetVibe.ApiService.WeatherMap;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;



namespace DotnetVibe.ApiService.Tests.Integration;



public sealed class ForecastRateLimitWebApplicationFactory : WebApplicationFactory<Program>

{

    private readonly string _databaseName = $"Testing-{Guid.NewGuid():N}";



    public FakeWeatherProvider FakeWeatherProvider { get; } = new();



    protected override void ConfigureWebHost(IWebHostBuilder builder)

    {

        IntegrationTestHostBuilder.Configure(

            builder,

            _databaseName,

            services => services.AddSingleton<IWeatherProvider>(FakeWeatherProvider));



        builder.ConfigureAppConfiguration((_, config) =>

        {

            config.AddInMemoryCollection(new Dictionary<string, string?>

            {

                ["WeatherMap:ForecastRateLimitPermitsPerMinute"] = "2"

            });

        });

    }

}



public sealed class ForecastRateLimitEndpointTests(ForecastRateLimitWebApplicationFactory factory)

    : IClassFixture<ForecastRateLimitWebApplicationFactory>

{

    [Fact]

    public async Task GetForecast_returns_429_when_rate_limit_exceeded()

    {

        factory.FakeWeatherProvider.NextForecast = SampleForecast();

        var client = factory.CreateClient();



        var first = await client.GetAsync("/weather-map/forecast?latitude=52.52&longitude=13.41");

        var second = await client.GetAsync("/weather-map/forecast?latitude=48.13&longitude=11.58");

        var third = await client.GetAsync("/weather-map/forecast?latitude=50.11&longitude=8.68");



        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);

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

}


