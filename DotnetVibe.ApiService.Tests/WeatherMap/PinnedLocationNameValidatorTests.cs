using DotnetVibe.ApiService.WeatherMap;

namespace DotnetVibe.ApiService.Tests.WeatherMap;

public sealed class PinnedLocationNameValidatorTests
{
    [Fact]
    public void ValidateAndNormalize_returns_trimmed_name()
    {
        var result = PinnedLocationNameValidator.ValidateAndNormalize("  Home  ");
        Assert.Equal("Home", result);
    }

    [Fact]
    public void ValidateAndNormalize_rejects_null_name()
    {
        Assert.Throws<InvalidPinnedLocationNameException>(
            () => PinnedLocationNameValidator.ValidateAndNormalize(null!));
    }

    [Fact]
    public void ValidateAndNormalize_rejects_empty_name()
    {
        Assert.Throws<InvalidPinnedLocationNameException>(
            () => PinnedLocationNameValidator.ValidateAndNormalize("   "));
    }

    [Fact]
    public void ValidateAndNormalize_rejects_name_over_max_length()
    {
        var longName = new string('a', PinnedLocationNameValidator.MaxLength + 1);
        Assert.Throws<InvalidPinnedLocationNameException>(
            () => PinnedLocationNameValidator.ValidateAndNormalize(longName));
    }
}
