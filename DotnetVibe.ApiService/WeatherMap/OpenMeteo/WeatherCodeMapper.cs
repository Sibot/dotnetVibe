namespace DotnetVibe.ApiService.WeatherMap.OpenMeteo;

internal static class WeatherCodeMapper
{
    public static string ToSummary(int weatherCode) => weatherCode switch
    {
        0 => "Clear sky",
        1 => "Mainly clear",
        2 => "Partly cloudy",
        3 => "Overcast",
        45 or 48 => "Fog",
        51 or 53 or 55 => "Drizzle",
        61 => "Slight rain",
        63 => "Moderate rain",
        65 => "Heavy rain",
        71 => "Slight snow",
        73 => "Moderate snow",
        75 => "Heavy snow",
        95 => "Thunderstorm",
        _ => "Unknown"
    };
}
