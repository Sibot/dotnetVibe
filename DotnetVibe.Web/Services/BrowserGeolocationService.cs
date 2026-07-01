using Microsoft.JSInterop;

namespace DotnetVibe.Web.Services;

public interface IGeolocationService
{
    Task<BrowserGeoPosition?> GetCurrentPositionAsync();
}

public sealed record BrowserGeoPosition(double Latitude, double Longitude);

public sealed class BrowserGeolocationService(IJSRuntime jsRuntime) : IGeolocationService
{
    public async Task<BrowserGeoPosition?> GetCurrentPositionAsync()
    {
        try
        {
            await using var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/weatherMap.js");
            return await module.InvokeAsync<BrowserGeoPosition?>("getCurrentPosition");
        }
        catch (JSException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            // JS interop is not available during static SSR prerender.
            return null;
        }
    }
}
