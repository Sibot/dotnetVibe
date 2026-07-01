using DotnetVibe.ApiService.WeatherMap;

namespace DotnetVibe.ApiService.Tests.WeatherMap;

public sealed class GeoCoordinateValidatorTests
{
    [Theory]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    [InlineData(0, 0)]
    public void Validate_accepts_finite_coordinates_in_range(double latitude, double longitude)
    {
        GeoCoordinateValidator.Validate(latitude, longitude);
    }

    [Theory]
    [InlineData(double.NaN, 0)]
    [InlineData(0, double.NaN)]
    [InlineData(double.PositiveInfinity, 0)]
    [InlineData(0, double.NegativeInfinity)]
    public void Validate_rejects_non_finite_coordinates(double latitude, double longitude)
    {
        Assert.Throws<InvalidCoordinatesException>(
            () => GeoCoordinateValidator.Validate(latitude, longitude));
    }

    [Fact]
    public void Validate_rejects_latitude_out_of_range()
    {
        Assert.Throws<InvalidCoordinatesException>(() => GeoCoordinateValidator.Validate(91, 0));
    }
}
