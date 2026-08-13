namespace RemoteCommerce.Controllers;

/// <summary>Provides browser endpoints for authentication and first-administrator bootstrap.</summary>
[ApiController]
public sealed class AccountController(IMediator mediator, IAntiforgery antiforgery) : ControllerBase
{
    /// <summary>Renders the sign-in form.</summary><param name="returnUrl">The optional local URL to return to after authentication.</param><returns>An HTML sign-in document.</returns>
    [AllowAnonymous, HttpGet("/login")]
    public ContentResult Login([FromQuery] string? returnUrl = null)
    {
        var safeReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
        var token = HtmlEncode(antiforgery.GetAndStoreTokens(HttpContext).RequestToken ?? string.Empty);
        var html = """
            <!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>RemoteCommerce Sign in</title><style>body{font-family:system-ui;max-width:420px;margin:10vh auto;padding:24px}label{display:block;margin-top:16px}input{width:100%;padding:10px;box-sizing:border-box}button{margin-top:20px;padding:10px 18px}</style></head>
            <body><h1>RemoteCommerce</h1><h2>Sign in</h2><form method="post" action="/login"><input type="hidden" name="__RequestVerificationToken" value="__TOKEN__"><input type="hidden" name="returnUrl" value="__RETURN_URL__"><label>Email<input name="email" type="email" autocomplete="username" required></label><label>Password<input name="password" type="password" autocomplete="current-password" required></label><button type="submit">Sign in</button></form></body></html>
            """;
        return Html(html.Replace("__TOKEN__", token, StringComparison.Ordinal).Replace("__RETURN_URL__", HtmlEncode(safeReturnUrl), StringComparison.Ordinal));
    }

    /// <summary>Authenticates a user with ASP.NET Core Identity through the application command pipeline.</summary><param name="email">The user's email address.</param><param name="password">The user's password.</param><param name="returnUrl">The optional local URL to return to after authentication.</param><param name="cancellationToken">The cancellation token.</param><returns>A redirect to the requested local URL or an authentication failure response.</returns>
    [AllowAnonymous, HttpPost("/login"), ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginPost([FromForm] string email, [FromForm] string password, [FromForm] string? returnUrl, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new LoginCommand(email, password), cancellationToken);
            Response.Cookies.Append(RemoteCommerce.Infrastructure.Identity.JwtOptions.CookieName, result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = result.ExpiresAt
            });
            return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Invalid credentials.");
        }
    }

    /// <summary>Signs out the current authenticated user.</summary><param name="cancellationToken">The cancellation token.</param><returns>A redirect to the sign-in page.</returns>
    [Authorize, HttpPost("/account/logout"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await mediator.Send(new LogoutCommand(), cancellationToken);
        Response.Cookies.Delete(RemoteCommerce.Infrastructure.Identity.JwtOptions.CookieName);
        return Redirect("/login");
    }

    /// <summary>Renders the first-administrator setup form when no user exists.</summary><param name="cancellationToken">The cancellation token.</param><returns>An HTML setup document.</returns>
    [AllowAnonymous, HttpGet("/admin/setup")]
    public async Task<ContentResult> Setup(CancellationToken cancellationToken)
    {
        if (!await mediator.Send(new GetSetupStatusQuery(), cancellationToken)) return Html("<html><body><h1>Setup already completed</h1><a href='/login'>Sign in</a></body></html>");
        var token = HtmlEncode(antiforgery.GetAndStoreTokens(HttpContext).RequestToken ?? string.Empty);
        var html = """
            <!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>RemoteCommerce Setup</title><style>body{font-family:system-ui;max-width:520px;margin:10vh auto;padding:24px}label{display:block;margin-top:16px}input{width:100%;padding:10px;box-sizing:border-box}button{margin-top:20px;padding:10px 18px}</style></head>
            <body><h1>Initial administrator</h1><p>Create the first administrator. The password is processed by ASP.NET Core Identity and is never stored in plaintext.</p><form method="post" action="/admin/setup"><input type="hidden" name="__RequestVerificationToken" value="__TOKEN__"><label>Name<input name="displayName" maxlength="200" required></label><label>Email<input name="email" type="email" autocomplete="username" required></label><label>Password<input name="password" type="password" autocomplete="new-password" minlength="12" required></label><button type="submit">Create administrator</button></form></body></html>
            """;
        return Html(html.Replace("__TOKEN__", token, StringComparison.Ordinal));
    }

    /// <summary>Creates the first administrator through the MediatR application pipeline.</summary><param name="displayName">The administrator display name.</param><param name="email">The administrator email address.</param><param name="password">The administrator password.</param><param name="cancellationToken">The cancellation token.</param><returns>A redirect to the sign-in page.</returns>
    [AllowAnonymous, HttpPost("/admin/setup"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SetupPost([FromForm] string displayName, [FromForm] string email, [FromForm] string password, CancellationToken cancellationToken)
    {
        await mediator.Send(new BootstrapAdministratorCommand(displayName, email, password), cancellationToken);
        return Redirect("/login");
    }

    private static ContentResult Html(string content) => new() { ContentType = "text/html; charset=utf-8", Content = content };
    private static string HtmlEncode(string value) => WebUtility.HtmlEncode(value);
}
