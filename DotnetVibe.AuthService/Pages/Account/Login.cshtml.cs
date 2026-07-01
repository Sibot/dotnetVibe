using System.ComponentModel.DataAnnotations;

using DotnetVibe.Auth;
using DotnetVibe.AuthService.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotnetVibe.AuthService.Pages.Account;

public sealed class LoginModel(
    SignInManager<ApplicationUser> signInManager,
    IWebHostEnvironment environment) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public bool ShowDevAccounts => environment.IsDevelopment();

    public void OnGet(string? returnUrl = null) =>
        ReturnUrl = LocalReturnUrlValidator.IsLocalUrl(returnUrl) ? returnUrl : null;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = LocalReturnUrlValidator.ResolveLoginReturnUrl(returnUrl, ReturnUrl);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            Input.Email,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return LocalRedirect(ReturnUrl ?? "/");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }

    public sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
