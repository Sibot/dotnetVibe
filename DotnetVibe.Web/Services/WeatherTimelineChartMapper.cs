using DotnetVibe.Web.Components.Charts;

namespace DotnetVibe.Web.Services;

public static class WeatherTimelineChartMapper
{
    private const string CelsiusSeriesColor = "#0d6efd";

    public static IReadOnlyList<ChartSeries> ToChartSeries(IEnumerable<WeatherForecast> forecasts)
    {
        var points = forecasts
            .OrderBy(forecast => forecast.Date)
            .Select(forecast => new ChartDataPoint(forecast.Date, forecast.TemperatureC))
            .ToList();

        if (points.Count == 0)
        {
            return [];
        }

        return [new ChartSeries("Temperature (°C)", points, CelsiusSeriesColor)];
    }
}
