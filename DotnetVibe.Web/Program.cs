using DotnetVibe.Auth;
using DotnetVibe.Web;
using DotnetVibe.Web.Components;
using DotnetVibe.Web.Services;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSingleton<WeatherHubClient>();
builder.Services.AddWebAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddScoped<IGeolocationService, BrowserGeolocationService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        client.BaseAddress = new("https+http://apiservice");
    })
    .AddHttpMessageHandler<AccessTokenHandler>();

builder.Services.AddHttpClient<WeatherMapApiClient>(client =>
    {
        client.BaseAddress = new("https+http://apiservice");
    })
    .AddHttpMessageHandler<AccessTokenHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseSecurityHeaders(new SecurityHeadersOptions
{
    FrameOptions = "SAMEORIGIN",
    PermissionsPolicy = "geolocation=(self)",
    ContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https://*.tile.openstreetmap.org; " +
        "connect-src 'self' https+http://apiservice wss: ws:; " +
        "font-src 'self'; " +
        "frame-ancestors 'self'; " +
        "base-uri 'self'; " +
        "form-action 'self'",
    ConfigureContentSecurityPolicy = (context, csp) =>
    {
        var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
        var authority = configuration["Authentication:Authority"];
        var requestOrigin = $"{context.Request.Scheme}://{context.Request.Host.Value}";
        var isDevelopment = context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment();
        var developmentUrls = isDevelopment
            ? configuration["ASPNETCORE_URLS"]?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : null;

        return WebContentSecurityPolicyCustomizer.Apply(
            csp,
            authority,
            requestOrigin,
            isDevelopment,
            developmentUrls);
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.UseOutputCache();

app.UseStaticFiles();
app.MapStaticAssets();

app.MapGet("/login", (string? returnUrl) =>
    Results.Challenge(new AuthenticationProperties
    {
        RedirectUri = LocalReturnUrlValidator.Normalize(returnUrl)
    }));

app.MapPost("/logout", async (HttpContext context, IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(context);
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/"
    });
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
