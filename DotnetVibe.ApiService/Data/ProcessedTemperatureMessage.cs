namespace DotnetVibe.ApiService.Data;

public sealed class ProcessedTemperatureMessage
{
    public required string MessageId { get; set; }

    public DateTimeOffset ProcessedAt { get; set; }
}
