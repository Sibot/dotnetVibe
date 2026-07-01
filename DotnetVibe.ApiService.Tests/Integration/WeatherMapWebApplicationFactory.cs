using DotnetVibe.ApiService.WeatherMap;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;



namespace DotnetVibe.ApiService.Tests.Integration;



public sealed class WeatherMapWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime

{

    private readonly string _databaseName = $"Testing-{Guid.NewGuid():N}";



    public FakeWeatherProvider FakeWeatherProvider { get; } = new();



    protected override void ConfigureWebHost(IWebHostBuilder builder)

    {

        IntegrationTestHostBuilder.Configure(

            builder,

            _databaseName,

            services => services.AddSingleton<IWeatherProvider>(FakeWeatherProvider));

    }



    Task IAsyncLifetime.InitializeAsync()

    {

        ResetFakeWeatherProvider();

        return Task.CompletedTask;

    }



    Task IAsyncLifetime.DisposeAsync()

    {

        ResetFakeWeatherProvider();

        return Task.CompletedTask;

    }



    private void ResetFakeWeatherProvider()

    {

        FakeWeatherProvider.ShouldThrowWeatherProviderException = false;

        FakeWeatherProvider.NextForecast = null;

    }

}



public sealed class FakeWeatherProvider : IWeatherProvider

{

    public LocationForecast? NextForecast { get; set; }



    public bool ShouldThrowWeatherProviderException { get; set; }



    public Task<LocationForecast> GetForecastAsync(

        double latitude,

        double longitude,

        ForecastFetchOptions? options = null,

        CancellationToken cancellationToken = default)

    {

        if (ShouldThrowWeatherProviderException)

        {

            throw new WeatherProviderException("Open-Meteo returned 503 (ServiceUnavailable).");

        }



        if (NextForecast is null)

        {

            throw new InvalidOperationException("FakeWeatherProvider.NextForecast is not configured.");

        }



        return Task.FromResult(NextForecast);

    }

}


