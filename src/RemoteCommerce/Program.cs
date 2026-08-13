var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment() && !string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase)) builder.Configuration.AddUserSecrets<Program>(optional: true);

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
})
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<CommerceDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration("Jwt")
    .Validate(x => !string.IsNullOrWhiteSpace(x.Key) && x.Key.Length >= 32, "Jwt:Key must be supplied by deployment configuration and contain at least 32 characters.")
    .Validate(x => !string.IsNullOrWhiteSpace(x.Issuer), "Jwt:Issuer is required.")
    .Validate(x => !string.IsNullOrWhiteSpace(x.Audience), "Jwt:Audience is required.")
    .ValidateOnStart();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"] ?? string.Empty)), ValidateIssuer = true, ValidIssuer = jwt["Issuer"] ?? "RemoteCommerce", ValidateAudience = true, ValidAudience = jwt["Audience"] ?? "RemoteCommerce", ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30), NameClaimType = ClaimTypes.Name, RoleClaimType = ClaimTypes.Role };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context => { if (string.IsNullOrWhiteSpace(context.Token) && context.Request.Cookies.TryGetValue(JwtOptions.CookieName, out var token)) context.Token = token; return Task.CompletedTask; },
            OnChallenge = context =>
            {
                if (!context.Response.HasStarted && !context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) && !context.Request.Path.StartsWithSegments("/_blazor", StringComparison.OrdinalIgnoreCase) && !context.Request.Path.StartsWithSegments("/_framework", StringComparison.OrdinalIgnoreCase))
                {
                    var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
                    context.Response.Redirect($"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                    context.HandleResponse();
                }

                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var subject = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var securityStamp = context.Principal?.FindFirst("security_stamp")?.Value;
                if (subject is null || securityStamp is null) { context.Fail("The authentication token is incomplete."); return; }
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByIdAsync(subject);
                if (user is null || user.IsDisabled || !string.Equals(user.SecurityStamp, securityStamp, StringComparison.Ordinal)) context.Fail("The authentication token has been invalidated.");
            }
        };
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.Administrator, policy => policy.RequireRole("Administrator"))
    .AddPolicy(AuthorizationPolicies.ManageConfiguration, policy 
        => policy.RequireAssertion(context => context.User.IsInRole("Administrator") 
        || context.User.HasClaim("permission", AuthorizationPolicies.ManageConfiguration)))
    .AddPolicy(AuthorizationPolicies.ManageUsers, policy 
        => policy.RequireAssertion(context => context.User.IsInRole("Administrator") 
        || context.User.HasClaim("permission", AuthorizationPolicies.ManageUsers)))
    .AddPolicy(AuthorizationPolicies.ManageLocalization, policy 
        => policy.RequireAssertion(context => context.User.IsInRole("Administrator")
        || context.User.HasClaim("permission", AuthorizationPolicies.ManageLocalization)))
    .AddPolicy(AuthorizationPolicies.ManagePlugins, policy 
        => policy.RequireAssertion(context => context.User.IsInRole("Administrator") 
        || context.User.HasClaim("permission", AuthorizationPolicies.ManagePlugins)));

builder.Services.AddSingleton<IApplicationContext, HttpApplicationContext>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddScoped<LocalizationResourceService>();
builder.Services.AddScoped<ILocalizationResourceService>(sp => sp.GetRequiredService<LocalizationResourceService>());
builder.Services.AddScoped<ILocalizer, RemoteCommerceLocalizer>();
builder.Services.AddScoped<ISecretProvider, ConfigurationSecretProvider>();
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
builder.Services.AddSingleton<IAdminNavigationRegistry>(adminNavigation);

var pluginsRoot = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "plugins");
builder.Services.AddInstalledRemoteCommercePlugins(pluginsRoot, builder.Configuration);

var app = builder.Build();
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment()) app.UseHsts();
app.MapOpenApi("o/{v1}.json");

if (!app.Environment.IsProduction())
    app.MapScalarApiReference("s/rc", configuration =>
        configuration.WithTitle($"[{app.Environment.EnvironmentName}] RemoteCommerce API Reference")
            .WithOpenApiRoutePattern("/o/{documentName}.json")
            .WithTheme(ScalarTheme.Purple));
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization(new RequestLocalizationOptions { DefaultRequestCulture = new RequestCulture("pt-BR"), SupportedCultures = [new CultureInfo("en-US"), new CultureInfo("pt-BR")], SupportedUICultures = [new CultureInfo("en-US"), new CultureInfo("pt-BR")], RequestCultureProviders = [new QueryStringRequestCultureProvider(), new CookieRequestCultureProvider(), new AcceptLanguageHeaderRequestCultureProvider(), new SiteSettingsRequestCultureProvider(app.Services.GetRequiredService<IDbContextFactory<CommerceDbContext>>())] });
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapControllers();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
await app.RunAsync();
