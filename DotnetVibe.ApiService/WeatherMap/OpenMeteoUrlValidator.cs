namespace DotnetVibe.ApiService.WeatherMap;

public sealed class InvalidOpenMeteoBaseUrlException(string message) : Exception(message);

public static class OpenMeteoUrlValidator
{
    private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "api.open-meteo.com"
    };

    public static Uri ValidateAndCreate(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            throw new InvalidOpenMeteoBaseUrlException("Open-Meteo base URL must be an absolute URI.");
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOpenMeteoBaseUrlException("Open-Meteo base URL must use HTTPS.");
        }

        if (!AllowedHosts.Contains(uri.Host))
        {
            throw new InvalidOpenMeteoBaseUrlException(
                $"Open-Meteo base URL host '{uri.Host}' is not allowed.");
        }

        return uri;
    }
}
