namespace DotnetVibe.ApiService.WeatherMap;

public sealed class InvalidPinnedLocationNameException(string message) : Exception(message);

public static class PinnedLocationNameValidator
{
    public const int MaxLength = 100;

    public static string ValidateAndNormalize(string? name)
    {
        if (name is null)
        {
            throw new InvalidPinnedLocationNameException("Name is required.");
        }

        var normalized = name.Trim();
        if (normalized.Length == 0)
        {
            throw new InvalidPinnedLocationNameException("Name is required.");
        }

        if (normalized.Length > MaxLength)
        {
            throw new InvalidPinnedLocationNameException($"Name must be at most {MaxLength} characters.");
        }

        return normalized;
    }
}
