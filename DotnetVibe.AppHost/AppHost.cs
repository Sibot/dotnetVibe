using DotnetVibe.AppHost.Extensions;

const string projectName = "dotnetVibe";

var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIREPROXYENDPOINTS001
var sql = builder.AddSqlServer("sql", port: 51433)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpointProxySupport(false)
    .WithProjectGrouping(projectName);
#pragma warning restore ASPIREPROXYENDPOINTS001

var db = sql.AddDatabase("dotnetvibedb");

var serviceBus = builder.AddAzureServiceBus("messaging")
    .RunAsEmulator(emulator => emulator
        .WithLifetime(ContainerLifetime.Persistent)
        .WithProjectGrouping(projectName));

var temperatureQueue = serviceBus.AddServiceBusQueue("temperature-events");

var cache = builder.AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithProjectGrouping(projectName);
var apiService = builder.AddProject<Projects.DotnetVibe_ApiService>("apiservice")
    .WithReference(db)
    .WithReference(temperatureQueue)
    .WithReference(cache)
    .WaitFor(db)
    .WaitFor(serviceBus)
    .WaitFor(cache)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.DotnetVibe_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
