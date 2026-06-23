using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace DotnetVibe.AppHost.Extensions;

public static class ContainerResourceExtensions
{
    private const string ComposeProjectLabel = "com.docker.compose.project";

    /// <summary>
    /// Groups this container under a Docker Compose project name in Docker Desktop and similar tools.
    /// </summary>
    public static IResourceBuilder<T> WithProjectGrouping<T>(
        this IResourceBuilder<T> builder,
        string projectName)
        where T : ContainerResource =>
        builder.WithContainerRuntimeArgs("--label", $"{ComposeProjectLabel}={projectName}");

    /// <summary>
    /// Groups every container resource under a Docker Compose project name, including sidecars
    /// created internally by integrations such as the Service Bus emulator's SQL Server container.
    /// </summary>
    public static void WithProjectGroupingForAllContainers(
        this IDistributedApplicationBuilder builder,
        string projectName)
    {
        foreach (var container in builder.Resources.OfType<ContainerResource>())
        {
            builder.CreateResourceBuilder(container)
                .WithProjectGrouping(projectName);
        }
    }
}
