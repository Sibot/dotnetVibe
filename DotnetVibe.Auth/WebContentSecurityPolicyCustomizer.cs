namespace DotnetVibe.Auth;

public static class WebContentSecurityPolicyCustomizer
{
    public const string ConnectSrcBaseline = "connect-src 'self' https+http://apiservice wss: ws:";

    public const string FormActionBaseline = "form-action 'self'";

    public static string Apply(
        string contentSecurityPolicy,
        string? authenticationAuthority,
        string requestOrigin,
        bool isDevelopment,
        IEnumerable<string>? developmentUrls = null)
    {
        var csp = contentSecurityPolicy;

        if (!string.IsNullOrWhiteSpace(authenticationAuthority))
        {
            var authority = authenticationAuthority.TrimEnd('/');
            csp = csp.Replace(ConnectSrcBaseline, $"{ConnectSrcBaseline} {authority}");
        }

        var formActionSources = new List<string> { "'self'", requestOrigin };
        if (!string.IsNullOrWhiteSpace(authenticationAuthority))
        {
            formActionSources.Add(authenticationAuthority.TrimEnd('/'));
        }

        if (isDevelopment)
        {
            foreach (var url in developmentUrls ?? [])
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    formActionSources.Add(url.TrimEnd('/'));
                }
            }
        }

        return csp.Replace(
            FormActionBaseline,
            $"form-action {string.Join(' ', formActionSources.Distinct())}");
    }
}
