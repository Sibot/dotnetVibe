using System.Net.Http.Headers;

using DotnetVibe.Auth;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace DotnetVibe.Web.Services;

public sealed class AccessTokenHandler(AccessTokenProvider accessTokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await accessTokenProvider.GetAccessTokenAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

public static class AuthenticationExtensions
{
    public static IServiceCollection AddWebAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var authority = configuration["Authentication:Authority"]
            ?? throw new InvalidOperationException("Authentication:Authority is not configured.");

        var clientSecret = AuthConfiguration.GetWebClientSecret(configuration, environment);
        var requireHttpsMetadata = AuthConfiguration.GetRequireHttpsMetadata(configuration, environment);
        var secureCookies = environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = secureCookies;
                options.Cookie.SameSite = SameSiteMode.Lax;
            })
            .AddOpenIdConnect(options =>
            {
                options.Authority = authority;
                options.ClientId = AuthClients.Web;
                options.ClientSecret = clientSecret;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.MapInboundClaims = false;
                options.TokenValidationParameters.NameClaimType = "name";
                options.TokenValidationParameters.RoleClaimType = "role";
                options.RequireHttpsMetadata = requireHttpsMetadata;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("roles");
                options.Scope.Add("offline_access");
                options.Scope.Add(AuthScopes.Api);
            });

        services.AddSingleton(TimeProvider.System);
        services.AddHttpClient(nameof(OAuthTokenRefresher));
        services.AddSingleton<OAuthTokenRefresher>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.WarmUp, policy => policy.RequireRole(AuthRoles.User, AuthRoles.Admin));
            options.AddPolicy(AuthPolicies.AdjustTemperature, policy => policy.RequireRole(AuthRoles.Admin));
        });

        services.AddCascadingAuthenticationState();
        services.AddHttpContextAccessor();
        services.AddSingleton<CircuitAccessTokenStore>();
        services.AddSingleton<AccessTokenProvider>();
        services.AddScoped<CircuitHandler, AccessTokenCircuitHandler>();
        services.AddTransient<AccessTokenHandler>();

        return services;
    }
}
