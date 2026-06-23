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
}
