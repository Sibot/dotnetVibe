namespace DotnetVibe.ApiService.WeatherMap;

public abstract record LocationSelection
{
    public static readonly LocationSelection Browser = new BrowserSelection();

    public static LocationSelection Pinned(Guid pinnedLocationId) => new PinnedSelection(pinnedLocationId);

    public bool IsBrowser => this is BrowserSelection;

    public Guid? PinnedLocationId => this is PinnedSelection pinned ? pinned.Id : null;

    private sealed record BrowserSelection : LocationSelection;

    private sealed record PinnedSelection(Guid Id) : LocationSelection;
}
