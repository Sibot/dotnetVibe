using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DotnetVibe.Auth;

public static class AuthConfiguration
{
    public const string WebClientSecretKey = "Authentication:WebClientSecret";
    public const string DevSeedPasswordKey = "Authentication:DevSeedPassword";
    public const string RequireHttpsMetadataKey = "Authentication:RequireHttpsMetadata";

    private const string DevelopmentWebClientSecret = "web-frontend-secret-dev";
    private const string DevelopmentSeedPassword = "DevPassword123!";

    public static string GetWebClientSecret(IConfiguration configuration, IHostEnvironment environment)
    {
        var secret = configuration[WebClientSecretKey];
        if (!string.IsNullOrWhiteSpace(secret))
        {
            return secret;
        }

        if (environment.IsDevelopment())
        {
            return DevelopmentWebClientSecret;
        }

        throw new InvalidOperationException(
            $"{WebClientSecretKey} must be configured outside Development.");
    }

    public static string GetDevSeedPassword(IConfiguration configuration, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException("Dev seed password is only available in Development.");
        }

        return configuration[DevSeedPasswordKey] ?? DevelopmentSeedPassword;
    }

    public static bool GetRequireHttpsMetadata(IConfiguration configuration, IHostEnvironment environment) =>
        configuration.GetValue(RequireHttpsMetadataKey, !environment.IsDevelopment());
}
