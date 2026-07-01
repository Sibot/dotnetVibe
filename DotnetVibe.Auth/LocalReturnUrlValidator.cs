namespace DotnetVibe.Auth;

public static class LocalReturnUrlValidator
{
    public static string Normalize(string? returnUrl) =>
        IsLocalUrl(returnUrl) ? returnUrl! : "/";

    /// <summary>
    /// Resolves the post-login return URL from the query string and bound form field.
    /// Prefers a valid query value; otherwise falls back to the bound field from the hidden input.
    /// </summary>
    public static string? ResolveLoginReturnUrl(string? returnUrlParameter, string? boundReturnUrl)
    {
        var candidate = IsLocalUrl(returnUrlParameter) ? returnUrlParameter : boundReturnUrl;
        return IsLocalUrl(candidate) ? candidate : null;
    }

    public static bool IsLocalUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return false;
        }

        if (!returnUrl.StartsWith('/', StringComparison.Ordinal))
        {
            return false;
        }

        if (returnUrl.StartsWith("//", StringComparison.Ordinal)
            || returnUrl.StartsWith("/\\", StringComparison.Ordinal))
        {
            return false;
        }

        return !returnUrl.Contains("://", StringComparison.Ordinal);
    }
}
