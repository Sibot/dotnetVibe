namespace DotnetVibe.ApiService.WeatherMap;

public sealed record PinnedLocationDto(
    Guid Id,
    string Name,
    double Latitude,
    double Longitude,
    int SortOrder)
{
    public PinnedLocationInfo ToInfo() => new(Id, Name, Latitude, Longitude, SortOrder);
}
