namespace DotnetVibe.ApiService.Services;

internal static class WeatherForecastSummary
{
    public static string FromTemperature(int temperatureC) => temperatureC switch
    {
        < 15 => "Chilly",
        < 20 => "Cool",
        < 25 => "Mild",
        < 30 => "Warm",
        _ => "Hot",
    };
}
