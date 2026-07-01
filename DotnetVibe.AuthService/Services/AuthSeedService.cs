using DotnetVibe.Auth;
using DotnetVibe.AuthService.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using OpenIddict.Abstractions;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace DotnetVibe.AuthService.Services;

public sealed class AuthSeedService(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<AuthSeedService> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync(cancellationToken);

        await SeedRolesAsync(scope.ServiceProvider, cancellationToken);
        await SeedUsersAsync(scope.ServiceProvider, cancellationToken);
        await SeedOpenIddictAsync(scope.ServiceProvider, cancellationToken);
    }

    private static async Task SeedRolesAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[] { AuthRoles.User, AuthRoles.Admin })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private async Task SeedUsersAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await EnsureUserAsync(userManager, "user@dotnetvibe.local", AuthRoles.User, cancellationToken);
        await EnsureUserAsync(userManager, "admin@dotnetvibe.local", AuthRoles.Admin, cancellationToken);
    }

    private async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string role,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, AuthConfiguration.GetDevSeedPassword(configuration, environment));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create user '{email}': {string.Join(", ", result.Errors.Select(error => error.Description))}");
            }

            logger.LogInformation("Created development user {Email}", email);
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }

    private async Task SeedOpenIddictAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var scopeManager = services.GetRequiredService<IOpenIddictScopeManager>();
        if (await scopeManager.FindByNameAsync(AuthScopes.Api, cancellationToken) is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = AuthScopes.Api,
                DisplayName = "dotnetVibe API",
                Resources = { AuthResources.Api }
            }, cancellationToken);
        }

        var applicationManager = services.GetRequiredService<IOpenIddictApplicationManager>();
        var redirectUris = configuration.GetSection("OpenIddict:WebClient:RedirectUris").Get<string[]>() ?? [];
        var postLogoutRedirectUris = configuration.GetSection("OpenIddict:WebClient:PostLogoutRedirectUris").Get<string[]>() ?? [];

        if (redirectUris.Length is 0)
        {
            logger.LogWarning("No web client redirect URIs configured; OIDC login from the web app will fail until they are set.");
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = AuthClients.Web,
            ClientSecret = AuthConfiguration.GetWebClientSecret(configuration, environment),
            DisplayName = "dotnetVibe Web",
            ClientType = ClientTypes.Confidential,
            ConsentType = ConsentTypes.Implicit,
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope + Scopes.OfflineAccess,
                Permissions.Prefixes.Scope + AuthScopes.Api
            }
        };

        foreach (var redirectUri in redirectUris)
        {
            descriptor.RedirectUris.Add(new Uri(redirectUri));
        }

        foreach (var postLogoutRedirectUri in postLogoutRedirectUris)
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(postLogoutRedirectUri));
        }

        var application = await applicationManager.FindByClientIdAsync(AuthClients.Web, cancellationToken);
        if (application is null)
        {
            await applicationManager.CreateAsync(descriptor, cancellationToken);
        }
        else
        {
            await applicationManager.UpdateAsync(application, descriptor, cancellationToken);
        }
    }
}
