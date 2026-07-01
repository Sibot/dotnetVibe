using DotnetVibe.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DotnetVibe.Web.Services;

public sealed class AccessTokenProvider(
    IHttpContextAccessor httpContextAccessor,
    CircuitAccessTokenStore circuitAccessTokenStore,
    OAuthTokenRefresher oauthTokenRefresher,
    TimeProvider timeProvider)
{
    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var circuitId = CircuitContext.CurrentCircuitId;
        if (circuitId is not null)
        {
            var circuitToken = circuitAccessTokenStore.GetToken(circuitId);
            if (!string.IsNullOrWhiteSpace(circuitToken))
            {
                return circuitToken;
            }
        }

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated is not true)
        {
            return null;
        }

        var expiresAt = await httpContext.GetTokenAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            "expires_at");

        var accessToken = await httpContext.GetTokenAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            "access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            var authResult = await httpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            accessToken = authResult.Properties?.GetTokenValue("access_token");
            expiresAt ??= authResult.Properties?.GetTokenValue("expires_at");
        }

        if (OAuthTokenRefreshPolicy.ShouldRefresh(expiresAt, timeProvider.GetUtcNow()))
        {
            accessToken = await oauthTokenRefresher.RefreshAccessTokenAsync(cancellationToken)
                ?? accessToken;
        }

        if (!string.IsNullOrWhiteSpace(accessToken) && circuitId is not null)
        {
            circuitAccessTokenStore.SetToken(circuitId, accessToken);
        }

        return accessToken;
    }
}
