using DotnetVibe.Auth;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace DotnetVibe.ApiService.Tests.Security;
public sealed class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task UseSecurityHeaders_applies_configured_headers()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services => services.AddRouting())
                    .Configure(app =>
                    {
                        app.UseSecurityHeaders(new SecurityHeadersOptions
                        {
                            FrameOptions = "SAMEORIGIN",
                            ContentSecurityPolicy = "default-src 'self'",
                            PermissionsPolicy = "geolocation=(self)"
                        });
                        app.UseRouting();
                        app.UseEndpoints(endpoints => endpoints.MapGet("/", () => "ok"));
                    });
            })
            .StartAsync();

        var client = host.GetTestClient();
        var response = await client.GetAsync("/");

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("SAMEORIGIN", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("default-src 'self'", response.Headers.GetValues("Content-Security-Policy").Single());
        Assert.Equal("geolocation=(self)", response.Headers.GetValues("Permissions-Policy").Single());
    }

    [Fact]
    public async Task UseSecurityHeaders_applies_csp_customizer_for_oidc_connect_src()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddRouting();
                        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                            .AddInMemoryCollection(new Dictionary<string, string?>
                            {
                                ["Authentication:Authority"] = "https://localhost:7275/"
                            })
                            .Build());
                        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment { EnvironmentName = Environments.Production });
                    })
                    .Configure(app =>
                    {
                        app.UseSecurityHeaders(new SecurityHeadersOptions
                        {
                            ContentSecurityPolicy =
                                "connect-src 'self' https+http://apiservice wss: ws:; " +
                                "form-action 'self'",
                            ConfigureContentSecurityPolicy = (context, csp) =>
                                WebContentSecurityPolicyCustomizer.Apply(
                                    csp,
                                    context.RequestServices.GetRequiredService<IConfiguration>()["Authentication:Authority"],
                                    $"{context.Request.Scheme}://{context.Request.Host.Value}",
                                    isDevelopment: false)
                        });
                        app.UseRouting();
                        app.UseEndpoints(endpoints => endpoints.MapGet("/", () => "ok"));
                    });
            })
            .StartAsync();

        var client = host.GetTestClient();
        var response = await client.GetAsync("/");
        var csp = response.Headers.GetValues("Content-Security-Policy").Single();

        Assert.Contains("connect-src 'self' https+http://apiservice wss: ws: https://localhost:7275", csp, StringComparison.Ordinal);
        Assert.Contains("form-action 'self'", csp, StringComparison.Ordinal);
        Assert.Contains("https://localhost:7275", csp, StringComparison.Ordinal);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;

        public string ApplicationName { get; set; } = "Test";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
