namespace RemoteCommerce_RemoteAdSense;

/// <summary>Registers RemoteAdSense and its placement persistence.</summary>
public sealed class PluginEntry : IRemoteCommercePlugin, IRemoteCommercePluginPersistence
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, PluginManifest manifest, IConfiguration configuration)
    {
        services.AddControllers().AddApplicationPart(typeof(PluginEntry).Assembly);
        services.AddScoped<AdSenseService>();
        _ = manifest;
        _ = configuration;
    }

    /// <inheritdoc />
    public void ConfigurePersistence(IPluginPersistenceBuilder builder)
        => builder.AddDbContext(typeof(AdSenseDbContext), typeof(PluginEntry).Assembly.GetName().Name);
}

/// <summary>Persists RemoteAdSense placement configuration for the current store.</summary>
public sealed class AdSenseDbContext(DbContextOptions<AdSenseDbContext> options) : DbContext(options)
{
    /// <summary>Gets configured ad placements.</summary>
    public DbSet<AdPlacement> Placements => Set<AdPlacement>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("rc_plugin_remote_adsense");
        modelBuilder.Entity<AdPlacement>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SlotName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AdClient).HasMaxLength(256).IsRequired();
            entity.Property(x => x.AdSlot).HasMaxLength(256);
            entity.Property(x => x.Format).HasMaxLength(64);
            entity.HasIndex(x => x.SlotName).IsUnique();
            entity.HasQueryFilter(x => !x.IsDisabled);
        });
    }
}

/// <summary>Represents a Google AdSense placement without storing secrets.</summary>
public sealed class AdPlacement : IPluginSoftDeletable
{
    /// <summary>Gets or sets the placement identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>Gets or sets the stable placement name.</summary>
    public string SlotName { get; set; } = string.Empty;
    /// <summary>Gets or sets the public AdSense publisher client identifier.</summary>
    public string AdClient { get; set; } = string.Empty;
    /// <summary>Gets or sets the public AdSense slot identifier.</summary>
    public string? AdSlot { get; set; }
    /// <summary>Gets or sets the responsive format.</summary>
    public string Format { get; set; } = "auto";
    /// <summary>Gets or sets whether automatic ads are enabled for this placement.</summary>
    public bool IsAutomatic { get; set; }
    /// <inheritdoc />
    public bool IsDisabled { get; set; }
}

/// <summary>Generates safe AdSense placement markup from persisted public metadata.</summary>
public sealed class AdSenseService(AdSenseDbContext db)
{
    /// <summary>Gets all enabled placements.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The enabled placement metadata.</returns>
    public Task<List<AdPlacement>> GetPlacementsAsync(CancellationToken cancellationToken)
        => db.Placements.AsNoTracking().OrderBy(x => x.SlotName).ToListAsync(cancellationToken);

    /// <summary>Creates a placement.</summary>
    /// <param name="placement">The placement metadata.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The persisted placement.</returns>
    public async Task<AdPlacement> CreateAsync(AdPlacement placement, CancellationToken cancellationToken)
    {
        db.Placements.Add(placement);
        await db.SaveChangesAsync(cancellationToken);
        return placement;
    }

    /// <summary>Builds a script-free markup contract for a configured placement.</summary>
    /// <param name="placement">The placement.</param>
    /// <returns>Markup attributes suitable for the Google AdSense client script.</returns>
    public string BuildMarkup(AdPlacement placement)
    {
        var slot = string.IsNullOrWhiteSpace(placement.AdSlot) ? string.Empty : $" data-ad-slot=\"{System.Net.WebUtility.HtmlEncode(placement.AdSlot)}\"";
        return $"<ins class=\"adsbygoogle\" style=\"display:block\" data-ad-client=\"{System.Net.WebUtility.HtmlEncode(placement.AdClient)}\"{slot} data-ad-format=\"{System.Net.WebUtility.HtmlEncode(placement.Format)}\"></ins>";
    }
}

/// <summary>Exposes RemoteAdSense placement management under the plugin API namespace.</summary>
[ApiController]
[Route("api/rp/v1/remote-adsense")]
public sealed class AdSenseController(AdSenseService service) : ControllerBase
{
    /// <summary>Lists enabled AdSense placements.</summary>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The enabled placements.</returns>
    [HttpGet("placements")]
    public async Task<ActionResult<IReadOnlyList<AdPlacement>>> GetPlacements(CancellationToken cancellationToken)
        => Ok(await service.GetPlacementsAsync(cancellationToken));

    /// <summary>Creates an AdSense placement.</summary>
    /// <param name="placement">The placement metadata.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The persisted placement.</returns>
    [HttpPost("placements")]
    public async Task<ActionResult<AdPlacement>> CreatePlacement(AdPlacement placement, CancellationToken cancellationToken)
        => Ok(await service.CreateAsync(placement, cancellationToken));

    /// <summary>Returns renderable markup for a placement.</summary>
    /// <param name="id">The placement identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The HTML contract.</returns>
    [HttpGet("placements/{id:guid}/markup")]
    public async Task<ActionResult<string>> GetMarkup(Guid id, CancellationToken cancellationToken)
    {
        var placements = await service.GetPlacementsAsync(cancellationToken);
        var item = placements.SingleOrDefault(x => x.Id == id);
        return item is null ? NotFound() : Ok(service.BuildMarkup(item));
    }
}

/// <summary>Provides the first RemoteAdSense database migration.</summary>
[Migration("202608140002_InitialRemoteAdSense")]
public sealed class InitialRemoteAdSenseMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("AdPlacements", schema: "rc_plugin_remote_adsense", table => new
        {
            Id = table.Column<Guid>(nullable: false), SlotName = table.Column<string>(maxLength: 128, nullable: false), AdClient = table.Column<string>(maxLength: 256, nullable: false), AdSlot = table.Column<string>(maxLength: 256, nullable: true), Format = table.Column<string>(maxLength: 64, nullable: false), IsAutomatic = table.Column<bool>(nullable: false), IsDisabled = table.Column<bool>(nullable: false)
        }, constraints: table => table.PrimaryKey("PK_AdPlacements", x => x.Id));
        migrationBuilder.CreateIndex("IX_AdPlacements_SlotName", "AdPlacements", "SlotName", unique: true, schema: "rc_plugin_remote_adsense");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("AdPlacements", "rc_plugin_remote_adsense");
}
