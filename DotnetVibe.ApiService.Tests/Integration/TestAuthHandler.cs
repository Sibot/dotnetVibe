using System.Security.Claims;
using System.Text.Encodings.Web;

using DotnetVibe.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetVibe.ApiService.Tests.Integration;

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-User", out var userIdHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = userIdHeader.ToString();
        var claims = new List<Claim> { new("sub", userId) };

        if (Request.Headers.TryGetValue("X-Test-Roles", out var rolesHeader))
        {
            foreach (var role in rolesHeader.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }
        }
        else if (userId.Contains("admin", StringComparison.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(ClaimTypes.Role, AuthRoles.Admin));
            claims.Add(new Claim("role", AuthRoles.Admin));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Role, AuthRoles.User));
            claims.Add(new Claim("role", AuthRoles.User));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }
}
