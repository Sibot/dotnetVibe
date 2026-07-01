using DotnetVibe.Auth;

namespace DotnetVibe.ApiService.Tests.Security;

public sealed class WebContentSecurityPolicyCustomizerTests
{
    private const string BaseCsp =
        "default-src 'self'; " +
        "connect-src 'self' https+http://apiservice wss: ws:; " +
        "form-action 'self'";

    [Fact]
    public void Apply_adds_authentication_authority_to_connect_src()
    {
        var csp = WebContentSecurityPolicyCustomizer.Apply(
            BaseCsp,
            authenticationAuthority: "https://localhost:7275",
            requestOrigin: "https://localhost:7069",
            isDevelopment: false);

        Assert.Contains(
            "connect-src 'self' https+http://apiservice wss: ws: https://localhost:7275",
            csp,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_adds_authentication_authority_to_form_action()
    {
        var csp = WebContentSecurityPolicyCustomizer.Apply(
            BaseCsp,
            authenticationAuthority: "https://localhost:7275",
            requestOrigin: "https://localhost:7069",
            isDevelopment: false);

        Assert.Contains(
            "form-action 'self' https://localhost:7069 https://localhost:7275",
            csp,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_leaves_connect_src_unchanged_when_authority_is_missing()
    {
        var csp = WebContentSecurityPolicyCustomizer.Apply(
            BaseCsp,
            authenticationAuthority: null,
            requestOrigin: "https://localhost:7069",
            isDevelopment: false);

        Assert.Contains(WebContentSecurityPolicyCustomizer.ConnectSrcBaseline, csp, StringComparison.Ordinal);
        Assert.DoesNotContain("connect-src 'self' https+http://apiservice wss: ws: https://", csp, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_includes_development_urls_in_form_action()
    {
        var csp = WebContentSecurityPolicyCustomizer.Apply(
            BaseCsp,
            authenticationAuthority: "https://localhost:7275",
            requestOrigin: "https://localhost:7069",
            isDevelopment: true,
            developmentUrls: ["https://localhost:7069", "http://localhost:5235"]);

        Assert.Contains("http://localhost:5235", csp, StringComparison.Ordinal);
    }
}
