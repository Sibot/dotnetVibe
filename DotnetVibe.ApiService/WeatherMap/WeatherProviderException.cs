namespace DotnetVibe.ApiService.WeatherMap;

public sealed class WeatherProviderException(string message, Exception? innerException = null)
    : Exception(message, innerException);
