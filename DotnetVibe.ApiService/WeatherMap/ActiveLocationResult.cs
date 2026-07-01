namespace DotnetVibe.ApiService.WeatherMap;

public sealed record ActiveLocationResult(
    LocationSource Source,
    GeoPoint Coordinates,
    Guid? PinnedLocationId,
    string? PinnedLocationName);
