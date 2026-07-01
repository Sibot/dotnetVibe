using System.Text.Json;

namespace DotnetVibe.ApiService.WeatherMap.OpenMeteo;

internal static class OpenMeteoResponseMapper
{
    public static LocationForecast Map(string json, double requestedLatitude, double requestedLongitude)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var latitude = root.GetProperty("latitude").GetDouble();
        var longitude = root.GetProperty("longitude").GetDouble();
        var current = root.GetProperty("current");
        var daily = root.GetProperty("daily");

        var observedAt = DateTimeOffset.Parse(current.GetProperty("time").GetString()!);
        var currentTemperature = current.GetProperty("temperature_2m").GetDouble();
        var currentWeatherCode = current.GetProperty("weather_code").GetInt32();
        var windSpeed = current.GetProperty("wind_speed_10m").GetDouble();

        var dailyForecasts = new List<DailyForecast>();
        var dates = daily.GetProperty("time").EnumerateArray().ToArray();
        var maxTemps = daily.GetProperty("temperature_2m_max").EnumerateArray().ToArray();
        var minTemps = daily.GetProperty("temperature_2m_min").EnumerateArray().ToArray();
        var weatherCodes = daily.GetProperty("weather_code").EnumerateArray().ToArray();

        for (var index = 0; index < dates.Length; index++)
        {
            dailyForecasts.Add(new DailyForecast(
                DateOnly.Parse(dates[index].GetString()!),
                maxTemps[index].GetDouble(),
                minTemps[index].GetDouble(),
                WeatherCodeMapper.ToSummary(weatherCodes[index].GetInt32())));
        }

        return new LocationForecast(
            new GeoPoint(latitude, longitude),
            new CurrentConditions(
                observedAt,
                currentTemperature,
                WeatherCodeMapper.ToSummary(currentWeatherCode),
                windSpeed),
            dailyForecasts);
    }
}
