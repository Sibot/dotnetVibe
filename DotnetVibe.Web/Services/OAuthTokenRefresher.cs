using System.Text.Json.Serialization;

using DotnetVibe.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DotnetVibe.Web.Services;

public sealed class OAuthTokenRefresher(
    IHttpContextAccessor httpContextAccessor,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    IHostEnvironment environment,
    TimeProvider timeProvider)
{
    public async Task<string?> RefreshAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated is not true)
        {
            return null;
        }

        var refreshToken = await httpContext.GetTokenAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            "refresh_token");
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var authority = configuration["Authentication:Authority"]
            ?? throw new InvalidOperationException("Authentication:Authority is not configured.");
        var clientSecret = AuthConfiguration.GetWebClientSecret(configuration, environment);

        var client = httpClientFactory.CreateClient(nameof(OAuthTokenRefresher));
        using var response = await client.PostAsync(
            new Uri(new Uri(authority.TrimEnd('/') + "/", UriKind.Absolute), "connect/token"),
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = AuthClients.Web,
                ["client_secret"] = clientSecret
            }),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload?.AccessToken))
        {
            return null;
        }

        var authenticateResult = await httpContext.AuthenticateAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
        if (authenticateResult.Properties is null || authenticateResult.Principal is null)
        {
            return null;
        }

        authenticateResult.Properties.UpdateTokenValue("access_token", payload.AccessToken);
        if (!string.IsNullOrWhiteSpace(payload.RefreshToken))
        {
            authenticateResult.Properties.UpdateTokenValue("refresh_token", payload.RefreshToken);
        }

        if (payload.ExpiresIn > 0)
        {
            var expiresAt = timeProvider.GetUtcNow().AddSeconds(payload.ExpiresIn);
            authenticateResult.Properties.UpdateTokenValue(
                "expires_at",
                expiresAt.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
        }

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            authenticateResult.Principal,
            authenticateResult.Properties);

        return payload.AccessToken;
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; init; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; init; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }
    }
}
