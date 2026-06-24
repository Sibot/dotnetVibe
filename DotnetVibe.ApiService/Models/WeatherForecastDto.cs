namespace DotnetVibe.ApiService.Models;

public record WeatherForecastDto(DateOnly Date, int TemperatureC)
{
    public string Summary => GetSummary(TemperatureC);

    public int TemperatureF => GetFahrenheit(TemperatureC);

    internal static string GetSummary(int temperatureC) => temperatureC switch
    {
        < 15 => "Chilly",
        < 20 => "Cool",
        < 25 => "Mild",
        < 30 => "Warm",
        _ => "Hot",
    };

    internal static int GetFahrenheit(int temperatureC) => 32 + (int)(temperatureC / 0.5556);
}
