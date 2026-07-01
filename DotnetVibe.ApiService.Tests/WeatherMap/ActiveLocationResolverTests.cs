using DotnetVibe.ApiService.WeatherMap;

namespace DotnetVibe.ApiService.Tests.WeatherMap;

public sealed class ActiveLocationResolverTests
{
    private static readonly GeoPoint BerlinBrowser = new(52.52, 13.41);
    private static readonly GeoPoint MunichPin = new(48.13, 11.58);
    private static readonly PinnedLocationInfo Home = new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Home", 48.13, 11.58, 0);
    private static readonly PinnedLocationInfo Work = new(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Work", 50.11, 8.68, 1);

    [Fact]
    public void AnonymousUser_UsesBrowserCoordinates()
    {
        var result = ActiveLocationResolver.Resolve(
            isAuthenticated: false,
            browserLocation: BerlinBrowser,
            pinnedLocations: [Home],
            selection: null);

        Assert.Equal(LocationSource.Browser, result.Source);
        Assert.Equal(BerlinBrowser, result.Coordinates);
        Assert.Null(result.PinnedLocationId);
    }

    [Fact]
    public void AuthenticatedUser_WithNoPins_UsesBrowserCoordinates()
    {
        var result = ActiveLocationResolver.Resolve(
            isAuthenticated: true,
            browserLocation: BerlinBrowser,
            pinnedLocations: [],
            selection: null);

        Assert.Equal(LocationSource.Browser, result.Source);
        Assert.Equal(BerlinBrowser, result.Coordinates);
    }

    [Fact]
    public void AuthenticatedUser_WithPins_SelectsPinnedLocation()
    {
        var result = ActiveLocationResolver.Resolve(
            isAuthenticated: true,
            browserLocation: BerlinBrowser,
            pinnedLocations: [Home, Work],
            selection: LocationSelection.Pinned(Home.Id));

        Assert.Equal(LocationSource.Pinned, result.Source);
        Assert.Equal(MunichPin, result.Coordinates);
        Assert.Equal(Home.Id, result.PinnedLocationId);
        Assert.Equal("Home", result.PinnedLocationName);
    }

    [Fact]
    public void AuthenticatedUser_ChoosesCurrentPosition_UsesBrowserCoordinates()
    {
        var result = ActiveLocationResolver.Resolve(
            isAuthenticated: true,
            browserLocation: BerlinBrowser,
            pinnedLocations: [Home],
            selection: LocationSelection.Browser);

        Assert.Equal(LocationSource.Browser, result.Source);
        Assert.Equal(BerlinBrowser, result.Coordinates);
        Assert.Null(result.PinnedLocationId);
    }

    [Fact]
    public void AuthenticatedUser_WithUnknownPinnedSelection_FallsBackToBrowser()
    {
        var unknownPinId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var result = ActiveLocationResolver.Resolve(
            isAuthenticated: true,
            browserLocation: BerlinBrowser,
            pinnedLocations: [Home],
            selection: LocationSelection.Pinned(unknownPinId));

        Assert.Equal(LocationSource.Browser, result.Source);
        Assert.Equal(BerlinBrowser, result.Coordinates);
        Assert.Null(result.PinnedLocationId);
    }

    [Fact]
    public void AuthenticatedUser_WithPins_NoneSelected_UsesFirstPinBySortOrder()
    {
        var result = ActiveLocationResolver.Resolve(
            isAuthenticated: true,
            browserLocation: BerlinBrowser,
            pinnedLocations: [Work, Home],
            selection: null);

        Assert.Equal(LocationSource.Pinned, result.Source);
        Assert.Equal(Home.Id, result.PinnedLocationId);
        Assert.Equal(MunichPin, result.Coordinates);
    }
}
