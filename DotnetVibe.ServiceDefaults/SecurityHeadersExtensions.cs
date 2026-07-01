namespace Microsoft.AspNetCore.Builder;

using Microsoft.AspNetCore.Http;

public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder app,
        SecurityHeadersOptions? options = null)
    {
        options ??= new SecurityHeadersOptions();
        return app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = options.FrameOptions;
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = options.PermissionsPolicy;

            var contentSecurityPolicy = options.ContentSecurityPolicy;
            if (!string.IsNullOrEmpty(contentSecurityPolicy))
            {
                if (options.ConfigureContentSecurityPolicy is not null)
                {
                    contentSecurityPolicy = options.ConfigureContentSecurityPolicy(context, contentSecurityPolicy);
                }

                headers.ContentSecurityPolicy = contentSecurityPolicy;
            }

            await next();
        });
    }
}

public sealed class SecurityHeadersOptions
{
    public string FrameOptions { get; init; } = "DENY";

    public string PermissionsPolicy { get; init; } = "camera=(), microphone=(), geolocation=()";

    public string? ContentSecurityPolicy { get; init; }

    public Func<HttpContext, string, string>? ConfigureContentSecurityPolicy { get; init; }
}
