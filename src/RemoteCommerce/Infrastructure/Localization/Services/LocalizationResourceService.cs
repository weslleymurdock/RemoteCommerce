namespace RemoteCommerce.Infrastructure.Localization.Services;

/// <summary>Validates, versions, stores, and activates XML localization resources.</summary>
/// <param name="dbFactory">The factory used to create read contexts.</param>
/// <param name="db">The scoped context used by transactional imports.</param>
/// <param name="environment">The host environment used to locate the application data directory.</param>
public sealed class LocalizationResourceService(
    IDbContextFactory<CommerceDbContext> dbFactory,
    CommerceDbContext db,
    IWebHostEnvironment environment) : ILocalizationResourceService
{
    private static readonly HashSet<string> SupportedCultures = ["en-US", "pt-BR"];

    /// <inheritdoc />
    public async Task<LocalizationResourceImportResult> ImportAsync(
        Stream content,
        string culture,
        string resourceType,
        Guid? importedByUserId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ValidateImportRequest(content, culture, resourceType);
        var entries = await ParseAsync(content, cancellationToken);
        var hash = await ComputeHashAsync(content, cancellationToken);
        content.Seek(0, SeekOrigin.Begin);

        var latestVersion = await db.LocalizationResources
            .Where(x => x.Culture == culture && x.ResourceType == resourceType)
            .Select(x => (int?)x.Version)
            .MaxAsync(cancellationToken) ?? 0;
        var version = latestVersion + 1;
        var directory = GetResourceDirectory(resourceType, culture);
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"v{version}.xml");

        try
        {
            await using (var file = File.Create(path))
            {
                await content.CopyToAsync(file, cancellationToken);
            }

            var activeResources = await db.LocalizationResources
                .Where(x => x.Culture == culture && x.ResourceType == resourceType && x.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var activeResource in activeResources)
            {
                activeResource.IsActive = false;
            }

            db.LocalizationResources.Add(new LocalizationResource
            {
                Culture = culture,
                ResourceType = resourceType,
                ContentHash = hash,
                Version = version,
                ImportedByUserId = importedByUserId,
                ImportedAt = DateTime.UtcNow,
                IsActive = true,
            });

            db.AuditLogs.Add(new AuditLog
            {
                UserId = importedByUserId,
                Actor = string.IsNullOrWhiteSpace(actor) ? "system" : actor,
                Operation = "localization.resource.import",
                Resource = resourceType,
                Result = "Success",
                Context = $"Culture={culture}; Version={version}; EntryCount={entries.Count}; Hash={hash}",
                CreatedAt = DateTime.UtcNow,
            });

            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            throw;
        }

        return new LocalizationResourceImportResult(
            culture,
            resourceType,
            version,
            entries.Count,
            hash);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LocalizationResourceSummary>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        await using var readDb = await dbFactory.CreateDbContextAsync(cancellationToken);

        return await readDb.LocalizationResources
            .AsNoTracking()
            .OrderByDescending(x => x.ImportedAt)
            .Select(x => new LocalizationResourceSummary(
                x.Id,
                x.Culture,
                x.ResourceType,
                x.Version,
                x.ImportedAt,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, string>?> GetActiveEntriesAsync(
        string resourceType,
        string culture,
        CancellationToken cancellationToken = default)
    {
        await using var readDb = await dbFactory.CreateDbContextAsync(cancellationToken);
        var resource = await readDb.LocalizationResources
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.ResourceType == resourceType
                    && x.Culture == culture
                    && x.IsActive,
                cancellationToken);

        if (resource is null)
        {
            return null;
        }
        
        var path = Path.Combine(
            GetResourceDirectory(resourceType, culture),
            $"v{resource.Version}.xml");

        if (!File.Exists(path))
        {
            return null;
        }

        await using var file = File.OpenRead(path);
        return await ParseAsync(file, cancellationToken);
    }

    private static void ValidateImportRequest(Stream content, string culture, string resourceType)
    {
        if (!SupportedCultures.Contains(culture))
        {
            throw new ArgumentException(
                "Only en-US and pt-BR resources are supported.",
                nameof(culture));
        }

        if (string.IsNullOrWhiteSpace(resourceType) || resourceType.Length > 500)
        {
            throw new ArgumentException(
                "A valid resource type is required.",
                nameof(resourceType));
        }

        if (!content.CanSeek)
        {
            throw new InvalidOperationException(
                "Localization resource streams must support seeking for safe persistence.");
        }
    }

    private string GetResourceDirectory(string resourceType, string culture)
    {
        var safeType = new string(
            resourceType
                .Select(ch => char.IsLetterOrDigit(ch) || ch is '.' or '_' ? ch : '_')
                .ToArray());

        return Path.Combine(
            environment.ContentRootPath,
            "App_Data",
            "localization",
            safeType,
            culture);
    }

    private static async Task<Dictionary<string, string>> ParseAsync(
        Stream content,
        CancellationToken cancellationToken)
    {
        content.Seek(0, SeekOrigin.Begin);
        var settings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreWhitespace = true,
        };
        var entries = new Dictionary<string, string>(StringComparer.Ordinal);

        using var reader = XmlReader.Create(content, settings);
        await reader.MoveToContentAsync();

        if (reader.NodeType != XmlNodeType.Element || reader.Name != "root")
        {
            throw new InvalidDataException(
                "The localization resource must contain a root element named 'root'.");
        }

        while (await reader.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.NodeType != XmlNodeType.Element || reader.Name != "data")
            {
                continue;
            }

            var key = reader.GetAttribute("name");
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidDataException(
                    "Every localization data element must define a name.");
            }

            if (!entries.TryAdd(key, string.Empty))
            {
                throw new InvalidDataException(
                    $"Duplicate localization resource key '{key}'.");
            }

            var hasValue = false;
            using var dataReader = reader.ReadSubtree();
            while (dataReader.Read())
            {
                if (dataReader.NodeType == XmlNodeType.Element && dataReader.Name == "value")
                {
                    entries[key] = await dataReader.ReadElementContentAsStringAsync();
                    hasValue = true;
                    break;
                }
            }

            if (!hasValue)
            {
                throw new InvalidDataException(
                    $"Localization resource key '{key}' does not contain a value element.");
            }
        }

        return entries;
    }

    private static async Task<string> ComputeHashAsync(
        Stream content,
        CancellationToken cancellationToken)
    {
        content.Seek(0, SeekOrigin.Begin);
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(content, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
