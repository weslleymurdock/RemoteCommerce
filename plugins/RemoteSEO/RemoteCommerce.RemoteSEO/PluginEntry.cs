namespace RemoteCommerce_RemoteSEO;

/// <summary>Registers the RemoteSEO plugin and its persistence boundary.</summary>
public sealed class PluginEntry : IRemoteCommercePlugin, IRemoteCommercePluginPersistence
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, PluginManifest manifest, IConfiguration configuration)
    {
        services.AddControllers().AddApplicationPart(typeof(PluginEntry).Assembly);
        services.AddScoped<SeoAnalysisService>();
        _ = configuration;
        _ = manifest;
    }

    /// <inheritdoc />
    public void ConfigurePersistence(IPluginPersistenceBuilder builder)
        => builder.AddDbContext(typeof(SeoDbContext), typeof(PluginEntry).Assembly.GetName().Name);
}

/// <summary>Persists RemoteSEO analysis records for the current store.</summary>
public sealed class SeoDbContext(DbContextOptions<SeoDbContext> options) : DbContext(options)
{
    /// <summary>Gets SEO page analysis records.</summary>
    public DbSet<SeoPageAnalysis> PageAnalyses => Set<SeoPageAnalysis>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("rc_plugin_remote_seo");
        modelBuilder.Entity<SeoPageAnalysis>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Route).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256);
            entity.Property(x => x.MetaDescription).HasMaxLength(1024);
            entity.Property(x => x.CanonicalUrl).HasMaxLength(1024);
            entity.Property(x => x.Score).IsRequired();
            entity.HasIndex(x => new { x.Route, x.CreatedAtUtc });
            entity.HasQueryFilter(x => !x.IsDisabled);
        });
    }
}

/// <summary>Represents a persisted SEO analysis for a rendered page or product route.</summary>
public sealed class SeoPageAnalysis : IPluginSoftDeletable
{
    /// <summary>Gets or sets the analysis identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>Gets or sets the analyzed route.</summary>
    public string Route { get; set; } = string.Empty;
    /// <summary>Gets or sets the page title.</summary>
    public string? Title { get; set; }
    /// <summary>Gets or sets the meta description.</summary>
    public string? MetaDescription { get; set; }
    /// <summary>Gets or sets the canonical URL.</summary>
    public string? CanonicalUrl { get; set; }
    /// <summary>Gets or sets the extracted word count.</summary>
    public int WordCount { get; set; }
    /// <summary>Gets or sets the SEO score from zero to one hundred.</summary>
    public int Score { get; set; }
    /// <summary>Gets or sets the recommendations serialized as JSON.</summary>
    public string RecommendationsJson { get; set; } = "[]";
    /// <summary>Gets or sets the UTC creation timestamp.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    /// <summary>Gets or sets the UTC update timestamp.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    /// <inheritdoc />
    public bool IsDisabled { get; set; }
}

/// <summary>Analyzes SEO metadata using deterministic, provider-independent rules.</summary>
public sealed class SeoAnalysisService(SeoDbContext db)
{
    /// <summary>Analyzes and persists one rendered page or product representation.</summary>
    /// <param name="request">The page metadata and rendered text.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The persisted analysis result.</returns>
    public async Task<SeoPageAnalysis> AnalyzeAsync(SeoAnalysisRequest request, CancellationToken cancellationToken)
    {
        var recommendations = new List<string>();
        var score = 100;
        var words = request.Content?.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
        if (string.IsNullOrWhiteSpace(request.Title)) { score -= 25; recommendations.Add("Add a descriptive title."); }
        else if (request.Title.Length is < 30 or > 60) { score -= 10; recommendations.Add("Keep the title between 30 and 60 characters."); }
        if (string.IsNullOrWhiteSpace(request.MetaDescription)) { score -= 20; recommendations.Add("Add a meta description."); }
        else if (request.MetaDescription.Length is < 70 or > 160) { score -= 10; recommendations.Add("Keep the meta description between 70 and 160 characters."); }
        if (words < 300) { score -= 15; recommendations.Add("Increase useful page content to at least 300 words."); }
        if (string.IsNullOrWhiteSpace(request.CanonicalUrl)) { score -= 10; recommendations.Add("Define a canonical URL."); }
        var result = new SeoPageAnalysis
        {
            Route = request.Route,
            Title = request.Title,
            MetaDescription = request.MetaDescription,
            CanonicalUrl = request.CanonicalUrl,
            WordCount = words,
            Score = Math.Clamp(score, 0, 100),
            RecommendationsJson = JsonSerializer.Serialize(recommendations)
        };
        db.PageAnalyses.Add(result);
        await db.SaveChangesAsync(cancellationToken);
        return result;
    }
}

/// <summary>Describes content submitted to the SEO analyzer.</summary>
public sealed record SeoAnalysisRequest(string Route, string? Title, string? MetaDescription, string? CanonicalUrl, string? Content);

/// <summary>Exposes the RemoteSEO analysis API under the plugin API namespace.</summary>
[ApiController]
[Route("api/rp/v1/remote-seo")]
public sealed class SeoController(SeoAnalysisService service) : ControllerBase
{
    /// <summary>Analyzes a rendered page or product representation.</summary>
    /// <param name="request">The metadata and content to analyze.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The persisted SEO analysis.</returns>
    [HttpPost("analyze")]
    public async Task<ActionResult<SeoPageAnalysis>> Analyze(SeoAnalysisRequest request, CancellationToken cancellationToken)
        => Ok(await service.AnalyzeAsync(request, cancellationToken));
}

/// <summary>Provides the first RemoteSEO database migration.</summary>
[Migration("202608140001_InitialRemoteSEO")]
public sealed class InitialRemoteSeoMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("SeoPageAnalyses", table => new
        {
            Id = table.Column<Guid>(nullable: false), Route = table.Column<string>(maxLength: 512, nullable: false), Title = table.Column<string>(maxLength: 256, nullable: true), MetaDescription = table.Column<string>(maxLength: 1024, nullable: true), CanonicalUrl = table.Column<string>(maxLength: 1024, nullable: true), WordCount = table.Column<int>(nullable: false), Score = table.Column<int>(nullable: false), RecommendationsJson = table.Column<string>(nullable: false), CreatedAtUtc = table.Column<DateTimeOffset>(nullable: false), UpdatedAtUtc = table.Column<DateTimeOffset>(nullable: false), IsDisabled = table.Column<bool>(nullable: false)
        }, constraints: table => table.PrimaryKey("PK_SeoPageAnalyses", x => x.Id), schema: "rc_plugin_remote_seo");
        migrationBuilder.CreateIndex("IX_SeoPageAnalyses_Route_CreatedAtUtc", "SeoPageAnalyses", new[] { "Route", "CreatedAtUtc" }, unique: false, schema: "rc_plugin_remote_seo");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("SeoPageAnalyses", "rc_plugin_remote_seo");
}
