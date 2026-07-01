using DotnetVibe.AppHost.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

const string projectName = "dotnetVibe";

var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIREPROXYENDPOINTS001
var sql = builder.AddSqlServer("sql", port: 51433)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpointProxySupport(false)
    .WithProjectGrouping(projectName);
#pragma warning restore ASPIREPROXYENDPOINTS001

var db = sql.AddDatabase("dotnetvibedb");
var authDb = sql.AddDatabase("authdb");

var serviceBus = builder.AddAzureServiceBus("messaging")
    .RunAsEmulator(emulator => emulator
        .WithLifetime(ContainerLifetime.Persistent)
        .WithProjectGrouping(projectName));

var temperatureQueue = serviceBus.AddServiceBusQueue("temperature-events");

var cache = builder.AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithProjectGrouping(projectName);

var authService = builder.AddProject<Projects.DotnetVibe_AuthService>("authservice")
    .WithReference(authDb)
    .WaitFor(authDb)
    .WithHttpHealthCheck("/health");

var apiService = builder.AddProject<Projects.DotnetVibe_ApiService>("apiservice")
    .WithReference(db)
    .WithReference(temperatureQueue)
    .WithReference(cache)
    .WithReference(authService)
    .WaitFor(db)
    .WaitFor(serviceBus)
    .WaitFor(cache)
    .WaitFor(authService)
    .WithEnvironment("Authentication__Authority", authService.GetEndpoint("https"))
    .WithHttpHealthCheck("/health");

var web = builder.AddProject<Projects.DotnetVibe_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WithReference(authService)
    .WaitFor(apiService)
    .WaitFor(authService)
    .WithEnvironment("Authentication__Authority", authService.GetEndpoint("https"));

authService
    .WithEnvironment("OpenIddict__WebClient__RedirectUris__0", ReferenceExpression.Create($"{web.GetEndpoint("https")}/signin-oidc"))
    .WithEnvironment("OpenIddict__WebClient__RedirectUris__1", ReferenceExpression.Create($"{web.GetEndpoint("http")}/signin-oidc"))
    .WithEnvironment("OpenIddict__WebClient__PostLogoutRedirectUris__0", ReferenceExpression.Create($"{web.GetEndpoint("https")}/signout-callback-oidc"))
    .WithEnvironment("OpenIddict__WebClient__PostLogoutRedirectUris__1", ReferenceExpression.Create($"{web.GetEndpoint("http")}/signout-callback-oidc"));

builder.WithProjectGroupingForAllContainers(projectName);

var app = builder.Build();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var logger = app.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("DotnetVibe.AppHost");

lifetime.ApplicationStopping.Register(() =>
    logger.LogInformation("Shutting down {ProjectName} AppHost...", projectName));

app.Run();
