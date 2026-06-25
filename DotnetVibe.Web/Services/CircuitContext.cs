namespace DotnetVibe.Web.Services;

internal static class CircuitContext
{
    private static readonly AsyncLocal<string?> CircuitId = new();

    public static string? CurrentCircuitId
    {
        get => CircuitId.Value;
        set => CircuitId.Value = value;
    }
}
