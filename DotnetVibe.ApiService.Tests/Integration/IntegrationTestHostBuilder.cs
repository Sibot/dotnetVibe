using DotnetVibe.ApiService;
using DotnetVibe.ApiService.Data;
using DotnetVibe.ApiService.WeatherMap;
using DotnetVibe.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotnetVibe.ApiService.Tests.Integration;

internal static class IntegrationTestHostBuilder
{
    public static void Configure(
        IWebHostBuilder builder,
        string databaseName,
        Action<IServiceCollection>? configureTestServices = null)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{ApiInfrastructureConfiguration.DatabaseConnectionName}"] = string.Empty,
                [$"ConnectionStrings:{ApiInfrastructureConfiguration.ServiceBusConnectionName}"] = string.Empty,
                [$"ConnectionStrings:{ApiInfrastructureConfiguration.CacheConnectionName}"] = string.Empty,
                ["Authentication:Authority"] = "https://test-auth.local",
                ["Authentication:RequireHttpsMetadata"] = "false"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton(_ =>
            {
                var connection = new SqliteConnection($"Data Source={databaseName};Mode=Memory;Cache=Shared");
                connection.Open();
                return connection;
            });
            services.AddDbContext<AppDbContext>((provider, options) =>
                options.UseSqlite(provider.GetRequiredService<SqliteConnection>()));
            services.AddDistributedMemoryCache();
            services.AddHostedService<IntegrationTestDatabaseInitializer>();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthPolicies.WarmUp, policy => policy.RequireRole(AuthRoles.User, AuthRoles.Admin));
                options.AddPolicy(AuthPolicies.AdjustTemperature, policy => policy.RequireRole(AuthRoles.Admin));
            });

            configureTestServices?.Invoke(services);
        });
    }
}
