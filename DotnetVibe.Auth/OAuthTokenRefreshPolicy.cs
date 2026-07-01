using System.Globalization;

namespace DotnetVibe.Auth;

public static class OAuthTokenRefreshPolicy
{
  private static readonly TimeSpan DefaultRefreshSkew = TimeSpan.FromMinutes(1);

  public static bool ShouldRefresh(string? expiresAtUtc, DateTimeOffset utcNow) =>
      ShouldRefresh(expiresAtUtc, utcNow, DefaultRefreshSkew);

  public static bool ShouldRefresh(string? expiresAtUtc, DateTimeOffset utcNow, TimeSpan refreshSkew)
  {
    if (string.IsNullOrWhiteSpace(expiresAtUtc))
    {
      return true;
    }

    if (!DateTimeOffset.TryParse(
            expiresAtUtc,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out var expiry))
    {
      return true;
    }

    return expiry <= utcNow.Add(refreshSkew);
  }
}
