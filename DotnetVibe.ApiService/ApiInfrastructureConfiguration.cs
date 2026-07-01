namespace DotnetVibe.ApiService;

public static class ApiInfrastructureConfiguration
{
    public const string DatabaseConnectionName = "dotnetvibedb";
    public const string ServiceBusConnectionName = "temperature-events";
    public const string CacheConnectionName = "cache";

    private static readonly string[] RequiredConnectionNames =
    [
        DatabaseConnectionName,
        ServiceBusConnectionName,
        CacheConnectionName
    ];

    public static bool IsConfigured(IConfiguration configuration) =>
        RequiredConnectionNames.All(name => HasConnectionString(configuration, name));

    public static void RequireForDeployment(IHostEnvironment environment, IConfiguration configuration)
    {
        if (environment.IsDevelopment())
        {
            return;
        }

        var missing = RequiredConnectionNames
            .Where(name => !HasConnectionString(configuration, name))
            .ToArray();

        if (missing.Length == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"API infrastructure is not configured for the '{environment.EnvironmentName}' environment. " +
            $"Missing connection strings: {string.Join(", ", missing)}.");
    }

    private static bool HasConnectionString(IConfiguration configuration, string name) =>
        !string.IsNullOrWhiteSpace(configuration.GetConnectionString(name));
}
