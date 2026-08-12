using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MudBlazor.Services;
using RemoteCommerce.Application.Administration;
using RemoteCommerce.Application.Identity;
using RemoteCommerce.Application.Localization;
using RemoteCommerce.Application.Security;
using RemoteCommerce.Application.Site;
using RemoteCommerce.Components;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Infrastructure.Persistence.Entities;
using RemoteCommerce.Plugins;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddLocalization();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddControllers();
builder.Services.AddMudServices();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddDbContextFactory<CommerceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Commerce")));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddRoles<ApplicationRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<CommerceDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.Administrator, policy =>
        policy.RequireRole("Administrator"));
    options.AddPolicy(AuthorizationPolicies.ManageConfiguration, policy =>
        policy.RequireAssertion(context => context.User.IsInRole("Administrator") || context.User.HasClaim("permission", AuthorizationPolicies.ManageConfiguration)));
    options.AddPolicy(AuthorizationPolicies.ManageUsers, policy =>
        policy.RequireAssertion(context => context.User.IsInRole("Administrator") || context.User.HasClaim("permission", AuthorizationPolicies.ManageUsers)));
    options.AddPolicy(AuthorizationPolicies.ManageLocalization, policy =>
        policy.RequireAssertion(context => context.User.IsInRole("Administrator") || context.User.HasClaim("permission", AuthorizationPolicies.ManageLocalization)));
    options.AddPolicy(AuthorizationPolicies.ManagePlugins, policy =>
        policy.RequireAssertion(context => context.User.IsInRole("Administrator") || context.User.HasClaim("permission", AuthorizationPolicies.ManagePlugins)));
});

builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddScoped<LocalizationResourceService>();
builder.Services.AddScoped<ILocalizationResourceService>(sp => sp.GetRequiredService<LocalizationResourceService>());
builder.Services.AddScoped<ILocalizer, RemoteCommerceLocalizer>();
builder.Services.AddScoped<ISecretProvider, ConfigurationSecretProvider>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

var adminNavigation = new AdminNavigationRegistry();
adminNavigation.Register(new AdminNavigationItem("Dashboard", "/", Icons.Material.Filled.Dashboard, 0));
adminNavigation.Register(new AdminNavigationItem("Site settings", "/admin/settings", Icons.Material.Filled.Settings, 10));
adminNavigation.Register(new AdminNavigationItem("Users", "/admin/users", Icons.Material.Filled.People, 20));
adminNavigation.Register(new AdminNavigationItem("Roles & permissions", "/admin/roles", Icons.Material.Filled.Security, 30));
adminNavigation.Register(new AdminNavigationItem("Localization", "/admin/localization", Icons.Material.Filled.Translate, 40));
adminNavigation.Register(new AdminNavigationItem("Security & configuration", "/admin/security", Icons.Material.Filled.Lock, 50));
adminNavigation.Register(new AdminNavigationItem("Plugins", "/plugins", Icons.Material.Filled.Extension, 60));
builder.Services.AddSingleton<IAdminNavigationRegistry>(adminNavigation);

var pluginsRoot = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "plugins");
builder.Services.AddInstalledRemoteCommercePlugins(pluginsRoot, builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.MapOpenApi("o/{v1}.json");

if (!app.Environment.IsProduction())
{
    app.MapScalarApiReference("s/rc", configuration =>
    {
        configuration.WithTitle($"[{app.Environment.EnvironmentName}] RemoteCommerce API Reference")
            .WithOpenApiRoutePattern("/o/{documentName}.json")
            .WithTheme(ScalarTheme.Purple)
            .AddHeadContent(@"
                <link href=""https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap"" rel=""stylesheet"" />
                <link href=""_content/MudBlazor/MudBlazor.min.css"" rel=""stylesheet"" />
                <script src=""_content/MudBlazor/MudBlazor.min.js""></script>
                <script>
                    document.addEventListener('DOMContentLoaded', () => {
                        document.body.classList.add('mud-application', 'mud-theme-primary');
                        const updateScalarTheme = () => {
                            const isDark = document.body.classList.contains('mud-dark-theme');
                            document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');
                        };
                        const observer = new MutationObserver(updateScalarTheme);
                        observer.observe(document.body, { attributes: true, attributeFilter: ['class'] });
                        updateScalarTheme();
                    });
                </script>
            ")
            .WithCustomCss(@"
                :root {
                    --scalar-background-1: var(--mud-palette-surface, #ffffff);
                    --scalar-background-2: var(--mud-palette-background, #f5f5f5);
                    --scalar-background-3: var(--mud-palette-background-gray, #e0e0e0);
                    --scalar-background-accent: var(--mud-palette-action-default-hover, rgba(0,0,0,0.04));
                    --scalar-color-1: var(--mud-palette-text-primary, #424242);
                    --scalar-color-2: var(--mud-palette-text-secondary, #616161);
                    --scalar-color-3: var(--mud-palette-text-disabled, #9e9e9e);
                    --scalar-color-accent: var(--mud-palette-primary, #594ae2);
                    --scalar-button-1: var(--mud-palette-primary, #594ae2);
                    --scalar-button-1-color: var(--mud-palette-primary-text, #ffffff);
                    --scalar-button-1-hover: var(--mud-palette-primary-darken, #3d2cc4);
                    --scalar-border-color: var(--mud-palette-lines-default, #e0e0e0);
                    --scalar-radius: var(--mud-default-borderradius, 4px);
                    --scalar-font: 'Roboto', sans-serif;
                    --scalar-font-code: 'Roboto Mono', monospace;
                }
                .mud-dark-theme, [data-theme='dark'] {
                    --scalar-background-1: var(--mud-palette-surface, #1e1e2d);
                    --scalar-background-2: var(--mud-palette-background, #151521);
                    --scalar-background-3: var(--mud-palette-background-gray, #27273a);
                    --scalar-color-1: var(--mud-palette-text-primary, #ffffff);
                    --scalar-color-2: var(--mud-palette-text-secondary, #a1a5b7);
                    --scalar-border-color: var(--mud-palette-lines-default, #2b2b40);
                }
                .scalar-api-reference {
                    font-family: var(--scalar-font);
                    background-color: var(--scalar-background-1);
                    color: var(--scalar-color-1);
                }
                .scalar-card, .section {
                    border-radius: var(--mud-default-borderradius, 4px) !important;
                    box-shadow: var(--mud-elevation-1, 0px 2px 1px -1px rgba(0,0,0,0.2)) !important;
                }
            ");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = [new CultureInfo("en-US"), new CultureInfo("pt-BR")],
    SupportedUICultures = [new CultureInfo("en-US"), new CultureInfo("pt-BR")],
    RequestCultureProviders =
    [
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider(),
        new CustomRequestCultureProvider(async context =>
        {
            var factory = context.RequestServices.GetRequiredService<IDbContextFactory<CommerceDbContext>>();
            var db = await factory.CreateDbContextAsync(context.RequestAborted);
            await using (db)
            {
                var settings = await db.SiteSettings.AsNoTracking().SingleOrDefaultAsync(x => x.Id == 1, context.RequestAborted);
                return settings is null ? null : new ProviderCultureResult(settings.Culture, settings.Culture);
            }
        }),
    ],
});
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
