namespace DotnetVibe.ApiService.WeatherMap;

public sealed record LocationForecast(
    GeoPoint Location,
    CurrentConditions Current,
    IReadOnlyList<DailyForecast> Daily);

public sealed record CurrentConditions(
    DateTimeOffset ObservedAt,
    double TemperatureCelsius,
    string Summary,
    double WindSpeedKph);

public sealed record DailyForecast(
    DateOnly Date,
    double TemperatureMaxCelsius,
    double TemperatureMinCelsius,
    string Summary);
