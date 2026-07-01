using DotnetVibe.ApiService.Security;
using DotnetVibe.ApiService.WeatherMap;

namespace DotnetVibe.ApiService.Tests.Security;

public sealed class OpenMeteoUrlValidatorTests
{
    [Fact]
    public void ValidateAndCreate_accepts_default_open_meteo_host()
    {
        var uri = OpenMeteoUrlValidator.ValidateAndCreate("https://api.open-meteo.com/");

        Assert.Equal("api.open-meteo.com", uri.Host);
        Assert.Equal(Uri.UriSchemeHttps, uri.Scheme);
    }

    [Theory]
    [InlineData("http://api.open-meteo.com/")]
    [InlineData("https://evil.example/")]
    [InlineData("not-a-uri")]
    [InlineData("https://localhost/")]
    public void ValidateAndCreate_rejects_unsafe_base_urls(string baseUrl)
    {
        Assert.ThrowsAny<InvalidOpenMeteoBaseUrlException>(
            () => OpenMeteoUrlValidator.ValidateAndCreate(baseUrl));
    }
}

public sealed class TemperatureDeltaValidatorTests
{
    [Theory]
    [InlineData(-50)]
    [InlineData(50)]
    [InlineData(1)]
    public void Validate_accepts_bounded_non_zero_delta(int delta)
    {
        Assert.Equal(delta, TemperatureDeltaValidator.Validate(delta));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    [InlineData(-51)]
    public void Validate_rejects_out_of_range_delta(int delta)
    {
        Assert.Throws<InvalidTemperatureDeltaException>(() => TemperatureDeltaValidator.Validate(delta));
    }
}
