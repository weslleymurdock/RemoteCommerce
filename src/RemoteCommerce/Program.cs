var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddLocalization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddControllers();
builder.Services.AddMudServices();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddDbContextFactory<CommerceDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Commerce")));

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

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.Administrator, policy => policy.RequireRole("Administrator"));
    options.AddPolicy(AuthorizationPolicies.ManageConfiguration, policy => policy.RequireAssertion(context => context.User.IsInRole("Administrator") || context.User.HasClaim("permission", AuthorizationPolicies.ManageConfiguration)));
    options.AddPolicy(AuthorizationPolicies.ManageUsers, policy => policy.RequireAssertion(context => context.User.IsInRole("Administrator") || context.User.HasClaim("permission", AuthorizationPolicies.ManageUsers)));
    options.AddPolicy(AuthorizationPolicies.ManageLocalization, policy => policy.RequireAssertion(context => context.User.IsInRole("Administrator") || context.User.HasClaim("permission", AuthorizationPolicies.ManageLocalization)));
    options.AddPolicy(AuthorizationPolicies.ManagePlugins, policy => policy.RequireAssertion(context => context.User.IsInRole("Administrator") || context.User.HasClaim("permission", AuthorizationPolicies.ManagePlugins)));
});

builder.Services.AddSingleton<IApplicationContext, HttpApplicationContext>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddScoped<LocalizationResourceService>();
builder.Services.AddScoped<ILocalizationResourceService>(sp => sp.GetRequiredService<LocalizationResourceService>());
builder.Services.AddScoped<ILocalizer, RemoteCommerceLocalizer>();
builder.Services.AddScoped<ISecretProvider, ConfigurationSecretProvider>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
    configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
    configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
    configuration.AddOpenBehavior(typeof(TransactionalBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

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
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.MapOpenApi("o/{v1}.json");
if (!app.Environment.IsProduction())
{
    app.MapScalarApiReference("s/rc", configuration => configuration.WithTitle($"[{app.Environment.EnvironmentName}] RemoteCommerce API Reference").WithOpenApiRoutePattern("/o/{documentName}.json").WithTheme(ScalarTheme.Purple).WithCustomCss(":root{--scalar-font:'Roboto',sans-serif}"));
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = [new CultureInfo("en-US"), new CultureInfo("pt-BR")],
    SupportedUICultures = [new CultureInfo("en-US"), new CultureInfo("pt-BR")],
    RequestCultureProviders = [new QueryStringRequestCultureProvider(), new CookieRequestCultureProvider(), new AcceptLanguageHeaderRequestCultureProvider(), new SiteSettingsRequestCultureProvider(app.Services.GetRequiredService<IDbContextFactory<CommerceDbContext>>())],
});
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapControllers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
await app.RunAsync();
