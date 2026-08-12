using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Application.Identity;
using RemoteCommerce.Infrastructure.Persistence.Entities;

namespace RemoteCommerce.Controllers;

/// <summary>Provides browser endpoints for authentication and first-administrator bootstrap.</summary>
[ApiController]
public sealed class AccountController(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    SignInManager<ApplicationUser> signInManager,
    IAntiforgery antiforgery) : ControllerBase
{
    /// <summary>Renders the sign-in form.</summary>
    /// <param name="returnUrl">The optional local URL to return to after authentication.</param>
    /// <returns>An HTML sign-in document.</returns>
    [AllowAnonymous]
    [HttpGet("/login")]
    public ContentResult Login([FromQuery] string? returnUrl = null)
    {
        var safeReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
        var token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken;
        return Html($"""
            <!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>RemoteCommerce Sign in</title>
            <style>body{{font-family:system-ui;max-width:420px;margin:10vh auto;padding:24px}}label{{display:block;margin-top:16px}}input{{width:100%;padding:10px;box-sizing:border-box}}button{{margin-top:20px;padding:10px 18px}}.error{{color:#b00020}}</style></head>
            <body><h1>RemoteCommerce</h1><h2>Sign in</h2><form method="post" action="/login"><input type="hidden" name="__RequestVerificationToken" value="{HtmlEncode(token ?? string.Empty)}"><input type="hidden" name="returnUrl" value="{HtmlEncode(safeReturnUrl)}">
            <label>Email<input name="email" type="email" autocomplete="username" required></label>
            <label>Password<input name="password" type="password" autocomplete="current-password" required></label>
            <button type="submit">Sign in</button></form></body></html>
            """);
    }

    /// <summary>Authenticates a user with ASP.NET Core Identity.</summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="returnUrl">The optional local URL to return to after authentication.</param>
    /// <returns>A redirect to the requested local URL or the dashboard.</returns>
    [AllowAnonymous]
    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginPost(
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] string? returnUrl)
    {
        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            return Unauthorized("Invalid credentials.");
        }

        var result = await signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return Unauthorized(result.IsLockedOut ? "Account temporarily locked." : "Invalid credentials.");
        }

        return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
    }

    /// <summary>Signs out the current authenticated user.</summary>
    /// <returns>A redirect to the sign-in page.</returns>
    [Authorize]
    [HttpPost("/account/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Redirect("/login");
    }

    /// <summary>Renders the first-administrator setup form when no user exists.</summary>
    /// <returns>An HTML setup document or a redirect when setup is already complete.</returns>
    [AllowAnonymous]
    [HttpGet("/admin/setup")]
    public async Task<ContentResult> Setup()
    {
        if (await userManager.Users.AnyAsync())
        {
            return Html("<html><body><h1>Setup already completed</h1><a href='/login'>Sign in</a></body></html>");
        }

        var token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken;
        return Html($"""
            <!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>RemoteCommerce Setup</title>
            <style>body{{font-family:system-ui;max-width:520px;margin:10vh auto;padding:24px}}label{{display:block;margin-top:16px}}input{{width:100%;padding:10px;box-sizing:border-box}}button{{margin-top:20px;padding:10px 18px}}</style></head>
            <body><h1>Initial administrator</h1><p>Create the first administrator. The password is processed by ASP.NET Core Identity and is never stored in plaintext.</p>
            <form method="post" action="/admin/setup"><input type="hidden" name="__RequestVerificationToken" value="{HtmlEncode(token ?? string.Empty)}"><label>Name<input name="displayName" maxlength="200" required></label><label>Email<input name="email" type="email" autocomplete="username" required></label><label>Password<input name="password" type="password" autocomplete="new-password" minlength="12" required></label><button type="submit">Create administrator</button></form></body></html>
            """);
    }

    /// <summary>Creates the first administrator and grants the baseline administration permissions.</summary>
    /// <param name="displayName">The administrator display name.</param>
    /// <param name="email">The administrator email address.</param>
    /// <param name="password">The administrator password.</param>
    /// <returns>A redirect to the dashboard or a validation response.</returns>
    [AllowAnonymous]
    [HttpPost("/admin/setup")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetupPost(
        [FromForm] string displayName,
        [FromForm] string email,
        [FromForm] string password)
    {
        if (await userManager.Users.AnyAsync())
        {
            return Conflict("Initial administrator setup has already been completed.");
        }

        const string roleName = "Administrator";
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new ApplicationRole
            {
                Name = roleName,
                Description = "Full access to RemoteCommerce administration.",
            });
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors.Select(x => x.Description));
            }
        }

        var user = new ApplicationUser
        {
            UserName = email.Trim(),
            Email = email.Trim(),
            DisplayName = displayName.Trim(),
            EmailConfirmed = true,
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return BadRequest(createResult.Errors.Select(x => x.Description));
        }

        var roleResultAfterCreation = await userManager.AddToRoleAsync(user, roleName);
        if (!roleResultAfterCreation.Succeeded)
        {
            return BadRequest(roleResultAfterCreation.Errors.Select(x => x.Description));
        }

        foreach (var permission in new[]
        {
            AuthorizationPolicies.ManageConfiguration,
            AuthorizationPolicies.ManageUsers,
            AuthorizationPolicies.ManageLocalization,
            AuthorizationPolicies.ManagePlugins,
        })
        {
            var claimResult = await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("permission", permission));
            if (!claimResult.Succeeded)
            {
                return BadRequest(claimResult.Errors.Select(x => x.Description));
            }
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return Redirect("/");
    }

    private static ContentResult Html(string content) => new()
    {
        ContentType = "text/html; charset=utf-8",
        Content = content,
    };

    private static string HtmlEncode(string value) => System.Net.WebUtility.HtmlEncode(value);
}
