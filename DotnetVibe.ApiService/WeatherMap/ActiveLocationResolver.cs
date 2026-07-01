namespace DotnetVibe.ApiService.WeatherMap;

public static class ActiveLocationResolver
{
    public static ActiveLocationResult Resolve(
        bool isAuthenticated,
        GeoPoint browserLocation,
        IReadOnlyList<PinnedLocationInfo> pinnedLocations,
        LocationSelection? selection)
    {
        if (selection?.IsBrowser == true)
        {
            return BrowserResult(browserLocation);
        }

        if (selection?.PinnedLocationId is Guid pinnedId)
        {
            var selected = pinnedLocations.FirstOrDefault(location => location.Id == pinnedId);
            if (selected is not null)
            {
                return PinnedResult(selected);
            }

            return BrowserResult(browserLocation);
        }

        if (isAuthenticated && pinnedLocations.Count > 0)
        {
            var defaultPin = pinnedLocations.OrderBy(location => location.SortOrder).First();
            return PinnedResult(defaultPin);
        }

        return BrowserResult(browserLocation);
    }

    private static ActiveLocationResult BrowserResult(GeoPoint browserLocation) =>
        new(LocationSource.Browser, browserLocation, null, null);

    private static ActiveLocationResult PinnedResult(PinnedLocationInfo location) =>
        new(
            LocationSource.Pinned,
            new GeoPoint(location.Latitude, location.Longitude),
            location.Id,
            location.Name);
}
