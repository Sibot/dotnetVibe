using DotnetVibe.ApiService.WeatherMap;

namespace DotnetVibe.ApiService.Tests.WeatherMap;

public sealed class PinnedLocationServiceTests
{
    private const string UserA = "user-a";
    private const string UserB = "user-b";

    [Fact]
    public async Task Create_adds_location_for_user()
    {
        await using var scope = await TestAppDbContextScope.CreateAsync();
        var service = new PinnedLocationService(scope.Db);

        var created = await service.CreateAsync(UserA, "Home", 52.52, 13.41);

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Home", created.Name);
        Assert.Equal(52.52, created.Latitude);
        Assert.Equal(13.41, created.Longitude);
        Assert.Equal(0, created.SortOrder);
    }

    [Fact]
    public async Task List_returns_only_calling_users_locations()
    {
        await using var scope = await TestAppDbContextScope.CreateAsync();
        var service = new PinnedLocationService(scope.Db);
        await service.CreateAsync(UserA, "Home", 52.52, 13.41);
        await service.CreateAsync(UserB, "Office", 48.13, 11.58);

        var userALocations = await service.ListAsync(UserA);

        Assert.Single(userALocations);
        Assert.Equal("Home", userALocations[0].Name);
    }

    [Fact]
    public async Task Update_renames_location_for_owner()
    {
        await using var scope = await TestAppDbContextScope.CreateAsync();
        var service = new PinnedLocationService(scope.Db);
        var created = await service.CreateAsync(UserA, "Home", 52.52, 13.41);

        var updated = await service.UpdateAsync(UserA, created.Id, "Apartment", null, null);

        Assert.NotNull(updated);
        Assert.Equal("Apartment", updated.Name);
    }

    [Fact]
    public async Task Update_returns_null_for_another_users_location()
    {
        await using var scope = await TestAppDbContextScope.CreateAsync();
        var service = new PinnedLocationService(scope.Db);
        var created = await service.CreateAsync(UserA, "Home", 52.52, 13.41);

        var updated = await service.UpdateAsync(UserB, created.Id, "Stolen", null, null);

        Assert.Null(updated);
    }

    [Fact]
    public async Task Delete_removes_location_for_owner()
    {
        await using var scope = await TestAppDbContextScope.CreateAsync();
        var service = new PinnedLocationService(scope.Db);
        var created = await service.CreateAsync(UserA, "Home", 52.52, 13.41);

        var deleted = await service.DeleteAsync(UserA, created.Id);
        var remaining = await service.ListAsync(UserA);

        Assert.True(deleted);
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task Delete_returns_false_for_another_users_location()
    {
        await using var scope = await TestAppDbContextScope.CreateAsync();
        var service = new PinnedLocationService(scope.Db);
        var created = await service.CreateAsync(UserA, "Home", 52.52, 13.41);

        var deleted = await service.DeleteAsync(UserB, created.Id);

        Assert.False(deleted);
        Assert.Single(await service.ListAsync(UserA));
    }

    [Fact]
    public async Task Create_rejects_invalid_coordinates()
    {
        await using var scope = await TestAppDbContextScope.CreateAsync();
        var service = new PinnedLocationService(scope.Db);

        await Assert.ThrowsAsync<InvalidCoordinatesException>(
            () => service.CreateAsync(UserA, "Bad", 91, 0));
    }

    [Fact]
    public async Task Create_rejects_empty_name()
    {
        await using var scope = await TestAppDbContextScope.CreateAsync();
        var service = new PinnedLocationService(scope.Db);

        await Assert.ThrowsAsync<InvalidPinnedLocationNameException>(
            () => service.CreateAsync(UserA, "  ", 52.52, 13.41));
    }

    [Fact]
    public async Task Create_rejects_when_user_reaches_pin_limit()
    {
        await using var scope = await TestAppDbContextScope.CreateAsync();
        var service = new PinnedLocationService(scope.Db);

        for (var index = 0; index < PinnedLocationLimits.MaxPerUser; index++)
        {
            await service.CreateAsync(UserA, $"Place {index}", 52.52, 13.41);
        }

        await Assert.ThrowsAsync<PinnedLocationLimitExceededException>(
            () => service.CreateAsync(UserA, "One too many", 52.52, 13.41));
    }
}
