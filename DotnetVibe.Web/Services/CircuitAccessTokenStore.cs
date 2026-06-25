using System.Collections.Concurrent;

namespace DotnetVibe.Web.Services;

public sealed class CircuitAccessTokenStore
{
    private readonly ConcurrentDictionary<string, string> tokens = new(StringComparer.Ordinal);

    public void SetToken(string circuitId, string accessToken) =>
        tokens[circuitId] = accessToken;

    public string? GetToken(string circuitId) =>
        tokens.TryGetValue(circuitId, out var accessToken) ? accessToken : null;

    public void RemoveToken(string circuitId) =>
        tokens.TryRemove(circuitId, out _);
}
