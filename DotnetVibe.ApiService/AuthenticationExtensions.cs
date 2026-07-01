using DotnetVibe.Auth;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DotnetVibe.ApiService;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddApiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var authority = configuration["Authentication:Authority"]
            ?? throw new InvalidOperationException("Authentication:Authority is not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = AuthResources.Api;
                options.MapInboundClaims = false;
                options.TokenValidationParameters.NameClaimType = "name";
                options.TokenValidationParameters.RoleClaimType = "role";
                options.RequireHttpsMetadata = AuthConfiguration.GetRequireHttpsMetadata(configuration, environment);
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.WarmUp, policy => policy.RequireRole(AuthRoles.User, AuthRoles.Admin));
            options.AddPolicy(AuthPolicies.AdjustTemperature, policy => policy.RequireRole(AuthRoles.Admin));
        });

        return services;
    }
}
