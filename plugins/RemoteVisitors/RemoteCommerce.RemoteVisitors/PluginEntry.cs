namespace RemoteCommerce_RemoteVisitors;

/// <summary>Registers RemoteVisitors tracking and persistence.</summary>
public sealed class PluginEntry : IRemoteCommercePlugin, IRemoteCommercePluginPersistence
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, PluginManifest manifest, IConfiguration configuration)
    {
        services.AddControllers().AddApplicationPart(typeof(PluginEntry).Assembly);
        services.AddScoped<VisitorTrackingService>();
        _ = manifest;
        _ = configuration;
    }

    /// <inheritdoc />
    public void ConfigurePersistence(IPluginPersistenceBuilder builder)
        => builder.AddDbContext(typeof(VisitorsDbContext), typeof(PluginEntry).Assembly.GetName().Name);
}

/// <summary>Persists visitor identities, visits and individual accesses.</summary>
public sealed class VisitorsDbContext(DbContextOptions<VisitorsDbContext> options) : DbContext(options)
{
    /// <summary>Gets visitor identities.</summary>
    public DbSet<Visitor> Visitors => Set<Visitor>();
    /// <summary>Gets visits.</summary>
    public DbSet<VisitorVisit> Visits => Set<VisitorVisit>();
    /// <summary>Gets individual accesses.</summary>
    public DbSet<VisitorAccess> Accesses => Set<VisitorAccess>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("rc_plugin_remote_visitors");
        modelBuilder.Entity<Visitor>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AnonymousKey).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.AnonymousKey).IsUnique();
            entity.HasQueryFilter(x => !x.IsDisabled);
        });
        modelBuilder.Entity<VisitorVisit>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EntryPath).HasMaxLength(1024);
            entity.Property(x => x.ExitPath).HasMaxLength(1024);
            entity.HasIndex(x => x.VisitorId);
        });
        modelBuilder.Entity<VisitorAccess>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Path).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Referrer).HasMaxLength(2048);
            entity.Property(x => x.UserAgent).HasMaxLength(2048);
            entity.HasIndex(x => new { x.VisitorId, x.AccessedAtUtc });
        });
    }
}

/// <summary>Represents an anonymized returning visitor identity.</summary>
public sealed class Visitor : IPluginSoftDeletable
{
    /// <summary>Gets or sets the visitor identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>Gets or sets the non-reversible visitor key.</summary>
    public string AnonymousKey { get; set; } = string.Empty;
    /// <summary>Gets or sets the first observed UTC timestamp.</summary>
    public DateTimeOffset FirstSeenUtc { get; set; } = DateTimeOffset.UtcNow;
    /// <summary>Gets or sets the most recent observed UTC timestamp.</summary>
    public DateTimeOffset LastSeenUtc { get; set; } = DateTimeOffset.UtcNow;
    /// <summary>Gets or sets the total number of visits.</summary>
    public int VisitCount { get; set; }
    /// <summary>Gets or sets the total number of accesses.</summary>
    public long AccessCount { get; set; }
    /// <inheritdoc />
    public bool IsDisabled { get; set; }
}

/// <summary>Represents a visit session separated from individual accesses.</summary>
public sealed class VisitorVisit
{
    /// <summary>Gets or sets the visit identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>Gets or sets the visitor identifier.</summary>
    public Guid VisitorId { get; set; }
    /// <summary>Gets or sets the UTC start timestamp.</summary>
    public DateTimeOffset StartedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    /// <summary>Gets or sets the UTC end timestamp.</summary>
    public DateTimeOffset? EndedAtUtc { get; set; }
    /// <summary>Gets or sets the entry route.</summary>
    public string? EntryPath { get; set; }
    /// <summary>Gets or sets the last observed route.</summary>
    public string? ExitPath { get; set; }
    /// <summary>Gets or sets the number of accesses in this visit.</summary>
    public int AccessCount { get; set; }
    /// <summary>Gets or sets the accumulated active duration in seconds.</summary>
    public long DurationSeconds { get; set; }
}

/// <summary>Represents one tracked request/access made during a visit.</summary>
public sealed class VisitorAccess
{
    /// <summary>Gets or sets the access identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>Gets or sets the visitor identifier.</summary>
    public Guid VisitorId { get; set; }
    /// <summary>Gets or sets the visit identifier.</summary>
    public Guid VisitId { get; set; }
    /// <summary>Gets or sets the UTC access timestamp.</summary>
    public DateTimeOffset AccessedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    /// <summary>Gets or sets the requested path.</summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>Gets or sets the referrer.</summary>
    public string? Referrer { get; set; }
    /// <summary>Gets or sets the user agent.</summary>
    public string? UserAgent { get; set; }
    /// <summary>Gets or sets the anonymized client network key.</summary>
    public string? NetworkKey { get; set; }
}

/// <summary>Tracks anonymous visitors, visit sessions and accesses.</summary>
public sealed class VisitorTrackingService(VisitorsDbContext db)
{
    private static readonly TimeSpan VisitTimeout = TimeSpan.FromMinutes(30);

    /// <summary>Records an access and starts a new visit when the previous visit expired.</summary>
    /// <param name="request">The incoming request telemetry.</param>
    /// <param name="visitorKey">The anonymous browser key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tracked access and visit identifiers.</returns>
    public async Task<(Guid VisitorId, Guid VisitId)> TrackAsync(TrackAccessRequest request, string visitorKey, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var visitor = await db.Visitors.SingleOrDefaultAsync(x => x.AnonymousKey == visitorKey, cancellationToken);
        if (visitor is null)
        {
            visitor = new Visitor { AnonymousKey = visitorKey, FirstSeenUtc = now, LastSeenUtc = now };
            db.Visitors.Add(visitor);
        }
        var visit = await db.Visits.Where(x => x.VisitorId == visitor.Id && x.StartedAtUtc >= now - VisitTimeout).OrderByDescending(x => x.StartedAtUtc).FirstOrDefaultAsync(cancellationToken);
        if (visit is null)
        {
            visit = new VisitorVisit { VisitorId = visitor.Id, StartedAtUtc = now, EntryPath = request.Path };
            visitor.VisitCount++;
            db.Visits.Add(visit);
        }
        visit.ExitPath = request.Path;
        visit.AccessCount++;
        visit.DurationSeconds = Math.Max(0, (long)(now - visit.StartedAtUtc).TotalSeconds);
        visitor.AccessCount++;
        visitor.LastSeenUtc = now;
        db.Accesses.Add(new VisitorAccess { VisitorId = visitor.Id, VisitId = visit.Id, AccessedAtUtc = now, Path = request.Path, Referrer = request.Referrer, UserAgent = request.UserAgent, NetworkKey = request.NetworkKey });
        await db.SaveChangesAsync(cancellationToken);
        return (visitor.Id, visit.Id);
    }

    /// <summary>Gets aggregate visitor statistics.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Visitor, visit, access, returning and average duration statistics.</returns>
    public async Task<VisitorStatistics> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var visitors = await db.Visitors.AsNoTracking().CountAsync(cancellationToken);
        var visits = await db.Visits.AsNoTracking().CountAsync(cancellationToken);
        var accesses = await db.Accesses.AsNoTracking().CountAsync(cancellationToken);
        var returning = await db.Visitors.AsNoTracking().CountAsync(x => x.VisitCount > 1, cancellationToken);
        var duration = await db.Visits.AsNoTracking().Select(x => (double?)x.DurationSeconds).AverageAsync(cancellationToken) ?? 0;
        return new VisitorStatistics(visitors, visits, accesses, returning, Math.Round(duration, 2));
    }
}

/// <summary>Describes one page access to track.</summary>
public sealed record TrackAccessRequest(string Path, string? Referrer, string? UserAgent, string? NetworkKey);

/// <summary>Summarizes RemoteVisitors telemetry.</summary>
public sealed record VisitorStatistics(int Visitors, int Visits, long Accesses, int ReturningVisitors, double AverageVisitDurationSeconds);

/// <summary>Exposes RemoteVisitors tracking and statistics APIs.</summary>
[ApiController]
[Route("api/rp/v1/remote-visitors")]
public sealed class VisitorsController(VisitorTrackingService service, IHttpContextAccessor httpContextAccessor) : ControllerBase
{
    /// <summary>Tracks one page access and establishes the anonymous visitor cookie when needed.</summary>
    /// <param name="request">The page access telemetry.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The visitor and visit identifiers.</returns>
    [HttpPost("track")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Track(TrackAccessRequest request, CancellationToken cancellationToken)
    {
        var context = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context is unavailable.");
        var key = context.Request.Cookies["rc_visitor"];
        if (string.IsNullOrWhiteSpace(key))
        {
            key = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            context.Response.Cookies.Append("rc_visitor", key, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Lax, Secure = context.Request.IsHttps, IsEssential = true, MaxAge = TimeSpan.FromDays(365) });
        }
        var networkKey = request.NetworkKey;
        if (string.IsNullOrWhiteSpace(networkKey) && context.Connection.RemoteIpAddress is { } ip) networkKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(ip.ToString())));
        var result = await service.TrackAsync(request with { NetworkKey = networkKey }, key, cancellationToken);
        return Ok(new { result.VisitorId, result.VisitId });
    }

    /// <summary>Returns aggregate visitor statistics.</summary>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>Visitor statistics.</returns>
    [HttpGet("statistics")]
    [Authorize]
    public async Task<ActionResult<VisitorStatistics>> Statistics(CancellationToken cancellationToken) => Ok(await service.GetStatisticsAsync(cancellationToken));
}

/// <summary>Provides the first RemoteVisitors database migration.</summary>
[Migration("202608140003_InitialRemoteVisitors")]
public sealed class InitialRemoteVisitorsMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        const string schema = "rc_plugin_remote_visitors";
        migrationBuilder.CreateTable("Visitors", schema: schema, table => new { Id = table.Column<Guid>(nullable: false), AnonymousKey = table.Column<string>(maxLength: 128, nullable: false), FirstSeenUtc = table.Column<DateTimeOffset>(nullable: false), LastSeenUtc = table.Column<DateTimeOffset>(nullable: false), VisitCount = table.Column<int>(nullable: false), AccessCount = table.Column<long>(nullable: false), IsDisabled = table.Column<bool>(nullable: false) }, constraints: table => table.PrimaryKey("PK_Visitors", x => x.Id));
        migrationBuilder.CreateTable("VisitorVisits", schema: schema, table => new { Id = table.Column<Guid>(nullable: false), VisitorId = table.Column<Guid>(nullable: false), StartedAtUtc = table.Column<DateTimeOffset>(nullable: false), EndedAtUtc = table.Column<DateTimeOffset>(nullable: true), EntryPath = table.Column<string>(maxLength: 1024, nullable: true), ExitPath = table.Column<string>(maxLength: 1024, nullable: true), AccessCount = table.Column<int>(nullable: false), DurationSeconds = table.Column<long>(nullable: false) }, constraints: table => table.PrimaryKey("PK_VisitorVisits", x => x.Id));
        migrationBuilder.CreateTable("VisitorAccesses", schema: schema, table => new { Id = table.Column<Guid>(nullable: false), VisitorId = table.Column<Guid>(nullable: false), VisitId = table.Column<Guid>(nullable: false), AccessedAtUtc = table.Column<DateTimeOffset>(nullable: false), Path = table.Column<string>(maxLength: 1024, nullable: false), Referrer = table.Column<string>(maxLength: 2048, nullable: true), UserAgent = table.Column<string>(maxLength: 2048, nullable: true), NetworkKey = table.Column<string>(nullable: true) }, constraints: table => table.PrimaryKey("PK_VisitorAccesses", x => x.Id));
        migrationBuilder.CreateIndex("IX_Visitors_AnonymousKey", "Visitors", "AnonymousKey", unique: true, schema: schema);
        migrationBuilder.CreateIndex("IX_VisitorVisits_VisitorId", "VisitorVisits", "VisitorId", schema: schema);
        migrationBuilder.CreateIndex("IX_VisitorAccesses_VisitorId_AccessedAtUtc", "VisitorAccesses", new[] { "VisitorId", "AccessedAtUtc" }, schema: schema);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        const string schema = "rc_plugin_remote_visitors";
        migrationBuilder.DropTable("VisitorAccesses", schema);
        migrationBuilder.DropTable("VisitorVisits", schema);
        migrationBuilder.DropTable("Visitors", schema);
    }
}
