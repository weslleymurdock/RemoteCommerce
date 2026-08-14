var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment()
    && !string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase))
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

var logDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "logs");
builder.Logging.AddRemoteCommerceFileLogging(builder.Services, logDirectory);
builder.Services.AddOpenApi();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddHttpContextAccessor();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddControllers();
builder.Services.AddMudServices();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<ISecretProvider, ConfigurationSecretProvider>();
builder.Services.AddSingleton<DatabaseProviderResolver>();
builder.Services.AddSingleton<RemoteCommerce.Application.Persistence.Abstractions.IDatabaseProvider>(services => services.GetRequiredService<DatabaseProviderResolver>().Resolve());
builder.Services.AddScoped<IDatabaseReplicationProvider, SqlServerReplicationProvider>();
builder.Services.AddSingleton<DatabaseSetupStateStore>();
builder.Services.AddScoped<IDatabaseSetupService, DatabaseSetupService>();
builder.Services.AddSingleton<MediaStorageProviderResolver>();
builder.Services.AddScoped<IMediaStorageProvider>(services => services.GetRequiredService<MediaStorageProviderResolver>().Resolve());
builder.Services.AddHostedService<ProviderConfigurationValidationService>();
builder.Services.AddDbContextFactory<CommerceDbContext>((services, options) => 
    options.UseSqlServer(
        services.GetRequiredService<RemoteCommerce.Application.Persistence.Abstractions.IDatabaseProvider>()
        .GetConnectionString(DatabaseEndpoint.Primary)));
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
}).AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<CommerceDbContext>()
.AddDefaultTokenProviders();
builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration("Jwt")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Key) && options.Key.Length >= 32, "Jwt:Key must be supplied by deployment configuration and contain at least 32 characters.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience is required.")
    .Validate(options => options.RefreshTokenExpirationDays is >= 1 and <= 365, "Jwt:RefreshTokenExpirationDays must be between 1 and 365.")
    .ValidateOnStart();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"] ?? string.Empty)),
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"] ?? "RemoteCommerce",
            ValidateAudience = true,
            ValidAudience = jwt["Audience"] ?? "RemoteCommerce",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrWhiteSpace(context.Token) && context.Request.Cookies.TryGetValue(JwtOptions.CookieName, out var token)) context.Token = token;
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var isFramework = context.Request.Path.StartsWithSegments("/_blazor") || context.Request.Path.StartsWithSegments("/_framework");
                if (!context.Response.HasStarted && !context.Request.Path.StartsWithSegments("/api") && !isFramework)
                {
                    context.Response.Redirect($"/login?returnUrl={Uri.EscapeDataString(context.Request.PathBase + context.Request.Path + context.Request.QueryString)}");
                    context.HandleResponse();
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var tokenType = context.Principal?.FindFirst("token_type")?.Value;
                var subject = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var securityStamp = context.Principal?.FindFirst("security_stamp")?.Value;
                if (!string.Equals(tokenType, JwtOptions.AccessTokenType, StringComparison.Ordinal) || subject is null || securityStamp is null)
                {
                    context.Fail("The authentication token is invalid.");
                    return;
                }
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByIdAsync(subject);
                if (user is null || user.IsDisabled || !string.Equals(user.SecurityStamp, securityStamp, StringComparison.Ordinal)) context.Fail("The authentication token has been invalidated.");
            }
        };
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.Administrator, policy => policy.RequireRole("Administrator"))
    .AddPolicy(AuthorizationPolicies.ManageConfiguration, policy => policy.RequireAssertion(context => context.User.IsInRole("Administrator") || context.User.HasClaim("permission", AuthorizationPolicies.ManageConfiguration)))
    .AddPolicy(AuthorizationPolicies.ManageUsers, policy => policy.RequireAssertion(context => context.User.IsInRole("Administrator") || context.User.HasClaim("permission", AuthorizationPolicies.ManageUsers)))
    .AddPolicy(AuthorizationPolicies.ManageLocalization, policy => policy.RequireAssertion(context => context.User.IsInRole("Administrator") || context.User.HasClaim("permission", AuthorizationPolicies.ManageLocalization)))
    .AddPolicy(AuthorizationPolicies.ManagePlugins, policy => policy.RequireAssertion(context => context.User.IsInRole("Administrator") || context.User.HasClaim("permission", AuthorizationPolicies.ManagePlugins)));
builder.Services.AddSingleton<IApplicationContext, HttpApplicationContext>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddScoped<LocalizationResourceService>();
builder.Services.AddScoped<ILocalizationResourceService>(serviceProvider => serviceProvider.GetRequiredService<LocalizationResourceService>());
builder.Services.AddScoped<ILocalizer, RemoteCommerceLocalizer>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<AccountHandlers>();
builder.Services.AddScoped<IEmailService<IdentityEmailMessage>, LoggingIdentityEmailService>();
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
    configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
    configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
    configuration.AddOpenBehavior(typeof(TransactionalBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
var adminNavigation = new AdminNavigationRegistry();
adminNavigation.Register(new AdminNavigationItem("Dashboard", "/admin", Icons.Material.Filled.Dashboard, 0));
adminNavigation.Register(new AdminNavigationItem("Site settings", "/admin/settings", Icons.Material.Filled.Settings, 10));
adminNavigation.Register(new AdminNavigationItem("Users", "/admin/users", Icons.Material.Filled.People, 20));
adminNavigation.Register(new AdminNavigationItem("Roles & permissions", "/admin/roles", Icons.Material.Filled.Security, 30));
adminNavigation.Register(new AdminNavigationItem("Localization", "/admin/localization", Icons.Material.Filled.Translate, 40));
adminNavigation.Register(new AdminNavigationItem("Security & configuration", "/admin/security", Icons.Material.Filled.Lock, 50));
adminNavigation.Register(new AdminNavigationItem("Plugins", "/admin/plugins", Icons.Material.Filled.Extension, 60));
adminNavigation.Register(new AdminNavigationItem("Application logs", "/admin/logs", Icons.Material.Filled.Terminal, 70));
adminNavigation.Register(new AdminNavigationItem("RemoteSEO", "/admin/remote-seo", Icons.Material.Filled.Search, 80));
adminNavigation.Register(new AdminNavigationItem("RemoteAdSense", "/admin/remote-adsense", Icons.Material.Filled.Campaign, 90));
adminNavigation.Register(new AdminNavigationItem("RemoteVisitors", "/admin/remote-visitors", Icons.Material.Filled.PeopleAlt, 100));
builder.Services.AddSingleton<IAdminNavigationRegistry>(adminNavigation);
var pluginsRoot = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "plugins");
builder.Services.AddInstalledRemoteCommercePlugins(pluginsRoot, builder.Configuration);
var app = builder.Build();
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.MapOpenApi("o/{v1}.json");
if (!app.Environment.IsProduction()) app.MapScalarApiReference("s/rc", configuration => configuration.WithTitle($"[{app.Environment.EnvironmentName}] RemoteCommerce API Reference").WithOpenApiRoutePattern("/o/{documentName}.json").WithTheme(ScalarTheme.Purple));
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = [new CultureInfo("pt-BR"), new CultureInfo("en-US")],
    SupportedUICultures = [new CultureInfo("pt-BR"), new CultureInfo("en-US")],
    RequestCultureProviders = [
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider(),
        new SiteSettingsRequestCultureProvider(app.Services.GetRequiredService<IDbContextFactory<CommerceDbContext>>())]
});
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapControllers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
await app.RunAsync();
