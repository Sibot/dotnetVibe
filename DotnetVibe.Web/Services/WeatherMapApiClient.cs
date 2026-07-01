namespace DotnetVibe.Web.Services;

public sealed class WeatherMapApiClient(HttpClient httpClient)
{
    public async Task<WeatherMapForecast?> GetForecastAsync(
        double latitude,
        double longitude,
        bool refresh = false,
        CancellationToken cancellationToken = default)
    {
        var refreshQuery = refresh ? "&refresh=true" : string.Empty;
        var response = await httpClient.GetAsync(
            $"/weather-map/forecast?latitude={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&longitude={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}{refreshQuery}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<WeatherMapForecast>(cancellationToken);
    }

    public async Task<IReadOnlyList<PinnedLocationModel>> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("/user/locations", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<PinnedLocationModel>>(cancellationToken) ?? [];
    }

    public async Task<PinnedLocationModel> CreateLocationAsync(
        string name,
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            "/user/locations",
            new CreatePinnedLocationRequest(name, latitude, longitude),
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PinnedLocationModel>(cancellationToken))!;
    }

    public async Task<PinnedLocationModel?> UpdateLocationAsync(
        Guid id,
        string name,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync(
            $"/user/locations/{id}",
            new UpdatePinnedLocationRequest(name, null, null),
            cancellationToken);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<PinnedLocationModel>(cancellationToken)
            : null;
    }

    public async Task DeleteLocationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/user/locations/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private sealed record CreatePinnedLocationRequest(string Name, double Latitude, double Longitude);

    private sealed record UpdatePinnedLocationRequest(string? Name, double? Latitude, double? Longitude);
}

public sealed record WeatherMapForecast(
    WeatherMapGeoPoint Location,
    WeatherMapCurrentConditions Current,
    IReadOnlyList<WeatherMapDailyForecast> Daily);

public sealed record WeatherMapGeoPoint(double Latitude, double Longitude);

public sealed record WeatherMapCurrentConditions(
    DateTimeOffset ObservedAt,
    double TemperatureCelsius,
    string Summary,
    double WindSpeedKph);

public sealed record WeatherMapDailyForecast(
    DateOnly Date,
    double TemperatureMaxCelsius,
    double TemperatureMinCelsius,
    string Summary);

public sealed record PinnedLocationModel(
    Guid Id,
    string Name,
    double Latitude,
    double Longitude,
    int SortOrder);
