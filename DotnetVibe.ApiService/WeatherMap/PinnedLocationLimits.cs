namespace DotnetVibe.ApiService.WeatherMap;

public static class PinnedLocationLimits
{
    public const int MaxPerUser = 5;
}

public sealed class PinnedLocationLimitExceededException()
    : Exception($"You can save at most {PinnedLocationLimits.MaxPerUser} pinned places.");
