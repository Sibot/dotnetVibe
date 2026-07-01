using DotnetVibe.Auth;
using DotnetVibe.AuthService.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotnetVibe.AuthService.Pages.Account;

public sealed class LogoutModel(SignInManager<ApplicationUser> signInManager) : PageModel
{
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        await signInManager.SignOutAsync();
        return LocalRedirect(LocalReturnUrlValidator.Normalize(returnUrl));
    }
}
