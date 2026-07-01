using DotnetVibe.Auth;

namespace DotnetVibe.ApiService.Tests.Security;

public sealed class OAuthTokenRefreshPolicyTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 30, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ShouldRefresh_returns_true_when_expires_at_is_missing()
    {
        Assert.True(OAuthTokenRefreshPolicy.ShouldRefresh(null, Now));
    }

    [Fact]
    public void ShouldRefresh_returns_false_when_token_is_still_valid()
    {
        var expiresAt = Now.AddMinutes(5).ToString("o");
        Assert.False(OAuthTokenRefreshPolicy.ShouldRefresh(expiresAt, Now));
    }

    [Fact]
    public void ShouldRefresh_returns_true_when_token_expires_within_skew_window()
    {
        var expiresAt = Now.AddSeconds(30).ToString("o");
        Assert.True(OAuthTokenRefreshPolicy.ShouldRefresh(expiresAt, Now));
    }
}
