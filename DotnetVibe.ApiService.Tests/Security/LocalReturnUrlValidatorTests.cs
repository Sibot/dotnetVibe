using DotnetVibe.Auth;

namespace DotnetVibe.ApiService.Tests.Security;

public sealed class LocalReturnUrlValidatorTests
{
    [Theory]
    [InlineData("/weather-map")]
    [InlineData("/weather")]
    [InlineData("/signin-oidc")]
    public void IsLocalUrl_accepts_relative_paths(string returnUrl)
    {
        Assert.True(LocalReturnUrlValidator.IsLocalUrl(returnUrl));
        Assert.Equal(returnUrl, LocalReturnUrlValidator.Normalize(returnUrl));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://evil.example/phish")]
    [InlineData("//evil.example/phish")]
    [InlineData("/\\evil.example/phish")]
    [InlineData("weather-map")]
    public void Normalize_rejects_unsafe_urls(string? returnUrl)
    {
        Assert.False(LocalReturnUrlValidator.IsLocalUrl(returnUrl));
        Assert.Equal("/", LocalReturnUrlValidator.Normalize(returnUrl));
    }

    [Fact]
    public void ResolveLoginReturnUrl_prefers_valid_query_over_bound_field()
    {
        var resolved = LocalReturnUrlValidator.ResolveLoginReturnUrl("/weather-map", "/counter");
        Assert.Equal("/weather-map", resolved);
    }

    [Fact]
    public void ResolveLoginReturnUrl_falls_back_to_bound_field_when_query_is_missing()
    {
        var resolved = LocalReturnUrlValidator.ResolveLoginReturnUrl(null, "/signin-oidc");
        Assert.Equal("/signin-oidc", resolved);
    }

    [Fact]
    public void ResolveLoginReturnUrl_rejects_unsafe_values()
    {
        Assert.Null(LocalReturnUrlValidator.ResolveLoginReturnUrl("//evil.example", null));
        Assert.Null(LocalReturnUrlValidator.ResolveLoginReturnUrl(null, "https://evil.example"));
    }
}
