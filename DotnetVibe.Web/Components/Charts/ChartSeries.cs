namespace DotnetVibe.Web.Components.Charts;

public sealed record ChartSeries(
    string Label,
    IReadOnlyList<ChartDataPoint> Points,
    string? Color = null);
