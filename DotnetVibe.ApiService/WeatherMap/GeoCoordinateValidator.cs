namespace DotnetVibe.ApiService.WeatherMap;

public sealed class InvalidCoordinatesException(string message) : Exception(message);

public static class GeoCoordinateValidator
{
    public static void Validate(double latitude, double longitude)
    {
        if (!IsFinite(latitude) || !IsFinite(longitude))
        {
            throw new InvalidCoordinatesException("Coordinates must be finite numbers.");
        }

        if (latitude is < -90 or > 90)
        {
            throw new InvalidCoordinatesException($"Latitude {latitude} is out of range (-90 to 90).");
        }

        if (longitude is < -180 or > 180)
        {
            throw new InvalidCoordinatesException($"Longitude {longitude} is out of range (-180 to 180).");
        }
    }

    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
