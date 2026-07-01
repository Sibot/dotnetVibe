namespace DotnetVibe.ApiService.Security;

public sealed class InvalidTemperatureDeltaException(string message) : Exception(message);

public static class TemperatureDeltaValidator
{
    public const int MaxAbsoluteDelta = 50;

    public static int Validate(int delta)
    {
        if (delta is 0)
        {
            throw new InvalidTemperatureDeltaException("Temperature delta cannot be zero.");
        }

        if (Math.Abs(delta) > MaxAbsoluteDelta)
        {
            throw new InvalidTemperatureDeltaException(
                $"Temperature delta must be between -{MaxAbsoluteDelta} and {MaxAbsoluteDelta}, exclusive of zero.");
        }

        return delta;
    }
}
