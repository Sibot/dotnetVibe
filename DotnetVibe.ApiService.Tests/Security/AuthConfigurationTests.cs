using DotnetVibe.Auth;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace DotnetVibe.ApiService.Tests.Security;

public sealed class AuthConfigurationTests
{
    [Fact]
    public void GetWebClientSecret_uses_development_fallback_in_development()
    {
        var configuration = new ConfigurationBuilder().Build();
        var environment = new TestHostEnvironment { EnvironmentName = Environments.Development };

        var secret = AuthConfiguration.GetWebClientSecret(configuration, environment);

        Assert.Equal("web-frontend-secret-dev", secret);
    }

    [Fact]
    public void GetWebClientSecret_requires_configuration_outside_development()
    {
        var configuration = new ConfigurationBuilder().Build();
        var environment = new TestHostEnvironment { EnvironmentName = Environments.Production };

        Assert.Throws<InvalidOperationException>(
            () => AuthConfiguration.GetWebClientSecret(configuration, environment));
    }

    [Fact]
    public void GetRequireHttpsMetadata_defaults_to_true_outside_development()
    {
        var configuration = new ConfigurationBuilder().Build();
        var environment = new TestHostEnvironment { EnvironmentName = Environments.Production };

        Assert.True(AuthConfiguration.GetRequireHttpsMetadata(configuration, environment));
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
