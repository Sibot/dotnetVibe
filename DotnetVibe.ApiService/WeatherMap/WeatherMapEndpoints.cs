using Microsoft.AspNetCore.Mvc;

namespace DotnetVibe.ApiService.WeatherMap;

public static class WeatherMapEndpoints
{
    public static IEndpointRouteBuilder MapWeatherMapEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/weather-map/forecast", GetForecastAsync)
            .RequireRateLimiting("forecast")
            .WithName("GetLocationForecast");

        var locations = endpoints.MapGroup("/user/locations")
            .RequireAuthorization()
            .RequireRateLimiting("pinned-locations");

        locations.MapGet("/", ListLocationsAsync);
        locations.MapPost("/", CreateLocationAsync);
        locations.MapPut("/{id:guid}", UpdateLocationAsync);
        locations.MapDelete("/{id:guid}", DeleteLocationAsync);

        return endpoints;
    }

    private static async Task<IResult> GetForecastAsync(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        IWeatherProvider weatherProvider,
        ILoggerFactory loggerFactory,
        [FromQuery] bool? refresh,
        CancellationToken cancellationToken)
    {
        try
        {
            GeoCoordinateValidator.Validate(latitude, longitude);
        }
        catch (InvalidCoordinatesException exception)
        {
            return TypedResults.BadRequest(new { error = exception.Message });
        }

        try
        {
            var options = refresh == true ? ForecastFetchOptions.Refresh : null;
            var forecast = await weatherProvider.GetForecastAsync(
                latitude,
                longitude,
                options,
                cancellationToken);
            return TypedResults.Ok(forecast);
        }
        catch (WeatherProviderException exception)
        {
            var logger = loggerFactory.CreateLogger("WeatherMap.Forecast");
            logger.LogWarning(exception, "Weather provider failed for ({Latitude}, {Longitude})", latitude, longitude);
            return TypedResults.Problem(
                detail: "Weather service unavailable. Please try again later.",
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static async Task<IResult> ListLocationsAsync(
        HttpContext httpContext,
        PinnedLocationService pinnedLocationService,
        CancellationToken cancellationToken)
    {
        var locations = await pinnedLocationService.ListAsync(GetUserId(httpContext), cancellationToken);
        return TypedResults.Ok(locations);
    }

    private static async Task<IResult> CreateLocationAsync(
        CreatePinnedLocationRequest request,
        HttpContext httpContext,
        PinnedLocationService pinnedLocationService,
        CancellationToken cancellationToken)
    {
        if (request.Name is null)
        {
            return TypedResults.BadRequest(new { error = "Name is required." });
        }

        try
        {
            var created = await pinnedLocationService.CreateAsync(
                GetUserId(httpContext),
                request.Name,
                request.Latitude,
                request.Longitude,
                cancellationToken);
            return TypedResults.Created($"/user/locations/{created.Id}", created);
        }
        catch (InvalidCoordinatesException exception)
        {
            return TypedResults.BadRequest(new { error = exception.Message });
        }
        catch (InvalidPinnedLocationNameException exception)
        {
            return TypedResults.BadRequest(new { error = exception.Message });
        }
        catch (PinnedLocationLimitExceededException exception)
        {
            return TypedResults.BadRequest(new { error = exception.Message });
        }
    }

    private static async Task<IResult> UpdateLocationAsync(
        Guid id,
        UpdatePinnedLocationRequest request,
        HttpContext httpContext,
        PinnedLocationService pinnedLocationService,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await pinnedLocationService.UpdateAsync(
                GetUserId(httpContext),
                id,
                request.Name,
                request.Latitude,
                request.Longitude,
                cancellationToken);

            return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated);
        }
        catch (InvalidCoordinatesException exception)
        {
            return TypedResults.BadRequest(new { error = exception.Message });
        }
        catch (InvalidPinnedLocationNameException exception)
        {
            return TypedResults.BadRequest(new { error = exception.Message });
        }
    }

    private static async Task<IResult> DeleteLocationAsync(
        Guid id,
        HttpContext httpContext,
        PinnedLocationService pinnedLocationService,
        CancellationToken cancellationToken)
    {
        var deleted = await pinnedLocationService.DeleteAsync(GetUserId(httpContext), id, cancellationToken);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    private static string GetUserId(HttpContext httpContext) =>
        httpContext.User.FindFirst("sub")?.Value
        ?? throw new InvalidOperationException("Authenticated request is missing sub claim.");
}

public sealed record CreatePinnedLocationRequest(string? Name, double Latitude, double Longitude);

public sealed record UpdatePinnedLocationRequest(string? Name, double? Latitude, double? Longitude);
