using System.Globalization;

namespace DotnetVibe.ApiService.WeatherMap.OpenMeteo;

public sealed class OpenMeteoWeatherProvider(HttpClient httpClient) : IWeatherProvider
{
    public async Task<LocationForecast> GetForecastAsync(
        double latitude,
        double longitude,
        ForecastFetchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var latitudeText = latitude.ToString(CultureInfo.InvariantCulture);
        var longitudeText = longitude.ToString(CultureInfo.InvariantCulture);
        var path =
            $"v1/forecast?latitude={latitudeText}&longitude={longitudeText}" +
            "&current=temperature_2m,weather_code,wind_speed_10m" +
            "&daily=temperature_2m_max,temperature_2m_min,weather_code" +
            "&timezone=auto";

        using var response = await httpClient.GetAsync(path, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new WeatherProviderException(
                $"Open-Meteo returned {(int)response.StatusCode} ({response.StatusCode}).");
        }

        try
        {
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return OpenMeteoResponseMapper.Map(json, latitude, longitude);
        }
        catch (Exception exception) when (exception is not WeatherProviderException)
        {
            throw new WeatherProviderException("Open-Meteo returned an invalid response.", exception);
        }
    }
}
