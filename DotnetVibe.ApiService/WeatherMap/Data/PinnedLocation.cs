namespace DotnetVibe.ApiService.WeatherMap.Data;

public sealed class PinnedLocation
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "";
    public string Name { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int SortOrder { get; set; }
}
