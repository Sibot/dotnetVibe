using Microsoft.AspNetCore.SignalR;

namespace DotnetVibe.ApiService.Hubs;

/// <summary>
/// Broadcasts weather forecast updates to all connected clients.
/// Anonymous access is intentional for the demo dashboard; production apps should require authentication.
/// </summary>
public class WeatherHub : Hub
{
}
