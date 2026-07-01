using System.Net;
using System.Net.Http.Json;

using DotnetVibe.ApiService.WeatherMap;

namespace DotnetVibe.ApiService.Tests.Integration;

public sealed class PinnedLocationsEndpointTests(WeatherMapWebApplicationFactory factory) : IClassFixture<WeatherMapWebApplicationFactory>
{
    [Fact]
    public async Task GetLocations_without_token_returns_401()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/user/locations");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostLocation_creates_and_returns_201()
    {
        var client = CreateAuthenticatedClient("user-a");
        var response = await client.PostAsJsonAsync("/user/locations", new CreatePinnedLocationRequest("Home", 52.52, 13.41));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PinnedLocationDto>();
        Assert.NotNull(body);
        Assert.Equal("Home", body.Name);
    }

    [Fact]
    public async Task PostLocation_returns_400_for_empty_name()
    {
        var client = CreateAuthenticatedClient("user-a");
        var response = await client.PostAsJsonAsync(
            "/user/locations",
            new CreatePinnedLocationRequest("  ", 52.52, 13.41));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostLocation_returns_400_for_null_name()
    {
        var client = CreateAuthenticatedClient("user-a");
        var response = await client.PostAsJsonAsync(
            "/user/locations",
            new { name = (string?)null, latitude = 52.52, longitude = 13.41 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutLocation_for_another_users_id_returns_404()
    {
        var ownerClient = CreateAuthenticatedClient("user-a");
        var createResponse = await ownerClient.PostAsJsonAsync(
            "/user/locations",
            new CreatePinnedLocationRequest("Home", 52.52, 13.41));
        var created = await createResponse.Content.ReadFromJsonAsync<PinnedLocationDto>();

        var otherClient = CreateAuthenticatedClient("user-b");
        var response = await otherClient.PutAsJsonAsync(
            $"/user/locations/{created!.Id}",
            new UpdatePinnedLocationRequest("Stolen", null, null));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteLocation_removes_item()
    {
        var client = CreateAuthenticatedClient("user-a");
        var createResponse = await client.PostAsJsonAsync(
            "/user/locations",
            new CreatePinnedLocationRequest("Home", 52.52, 13.41));
        var created = await createResponse.Content.ReadFromJsonAsync<PinnedLocationDto>();

        var deleteResponse = await client.DeleteAsync($"/user/locations/{created!.Id}");
        var listResponse = await client.GetAsync("/user/locations");
        var locations = await listResponse.Content.ReadFromJsonAsync<List<PinnedLocationDto>>();

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.NotNull(locations);
        Assert.Empty(locations);
    }

    private HttpClient CreateAuthenticatedClient(string userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", userId);
        return client;
    }

    private sealed record CreatePinnedLocationRequest(string Name, double Latitude, double Longitude);

    private sealed record UpdatePinnedLocationRequest(string? Name, double? Latitude, double? Longitude);
}
