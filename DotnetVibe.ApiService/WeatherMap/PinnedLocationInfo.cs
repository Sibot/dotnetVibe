namespace DotnetVibe.ApiService.WeatherMap;

public sealed record PinnedLocationInfo(
    Guid Id,
    string Name,
    double Latitude,
    double Longitude,
    int SortOrder);
