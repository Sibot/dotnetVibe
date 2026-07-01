using DotnetVibe.ApiService;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace DotnetVibe.ApiService.Tests;

public sealed class ApiInfrastructureConfigurationTests
{
    [Fact]
    public void IsConfigured_returns_false_when_connection_strings_are_missing()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.False(ApiInfrastructureConfiguration.IsConfigured(configuration));
    }

    [Fact]
    public void IsConfigured_returns_true_when_all_connection_strings_are_present()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{ApiInfrastructureConfiguration.DatabaseConnectionName}"] = "Server=sql;Database=db;",
                [$"ConnectionStrings:{ApiInfrastructureConfiguration.ServiceBusConnectionName}"] = "Endpoint=sb://test/",
                [$"ConnectionStrings:{ApiInfrastructureConfiguration.CacheConnectionName}"] = "localhost:6379"
            })
            .Build();

        Assert.True(ApiInfrastructureConfiguration.IsConfigured(configuration));
    }

    [Fact]
    public void RequireForDeployment_allows_missing_connection_strings_in_development()
    {
        var configuration = new ConfigurationBuilder().Build();
        var environment = new TestHostEnvironment { EnvironmentName = Environments.Development };

        var exception = Record.Exception(
            () => ApiInfrastructureConfiguration.RequireForDeployment(environment, configuration));

        Assert.Null(exception);
    }

    [Fact]
    public void RequireForDeployment_throws_in_production_when_connection_strings_are_missing()
    {
        var configuration = new ConfigurationBuilder().Build();
        var environment = new TestHostEnvironment { EnvironmentName = Environments.Production };

        var exception = Assert.Throws<InvalidOperationException>(
            () => ApiInfrastructureConfiguration.RequireForDeployment(environment, configuration));

        Assert.Contains(ApiInfrastructureConfiguration.DatabaseConnectionName, exception.Message, StringComparison.Ordinal);
        Assert.Contains(ApiInfrastructureConfiguration.ServiceBusConnectionName, exception.Message, StringComparison.Ordinal);
        Assert.Contains(ApiInfrastructureConfiguration.CacheConnectionName, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RequireForDeployment_allows_production_when_all_connection_strings_are_present()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{ApiInfrastructureConfiguration.DatabaseConnectionName}"] = "Server=sql;Database=db;",
                [$"ConnectionStrings:{ApiInfrastructureConfiguration.ServiceBusConnectionName}"] = "Endpoint=sb://test/",
                [$"ConnectionStrings:{ApiInfrastructureConfiguration.CacheConnectionName}"] = "localhost:6379"
            })
            .Build();
        var environment = new TestHostEnvironment { EnvironmentName = Environments.Production };

        var exception = Record.Exception(
            () => ApiInfrastructureConfiguration.RequireForDeployment(environment, configuration));

        Assert.Null(exception);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } =
            new PhysicalFileProvider(AppContext.BaseDirectory);
    }
}
