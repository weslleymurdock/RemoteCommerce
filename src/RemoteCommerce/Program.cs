using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using RemoteCommerce.Components;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Plugins;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddMudServices();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddDbContextFactory<CommerceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Commerce")));

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
                        // Adds mud-application class to body in order to activate MudBlazor escopes
                        document.body.classList.add('mud-application', 'mud-theme-primary');
                        // Ensures that Scalar ThemeMode do Scalar reflects actual MudBlazor dark state
                        const updateScalarTheme = () => {
                            const isDark = document.body.classList.contains('mud-dark-theme');
                            document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');
                        };
                        // Monitor classes changes at body to real time alternate theme
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
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
