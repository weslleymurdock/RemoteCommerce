namespace RemoteCommerce.Infrastructure.Persistence.Services;

/// <summary>Persists non-secret database setup state in an application-owned file.</summary>
/// <param name="configuration">The deployment configuration source.</param>
/// <param name="environment">The host environment used to resolve relative paths.</param>
public sealed class DatabaseSetupStateStore(
    IConfiguration configuration,
    IWebHostEnvironment environment)
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>Reads the current persisted setup document.</summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The persisted document, or <see langword="null"/> when setup has not been persisted.</returns>
    public async Task<DatabaseSetupStateDocument?> ReadAsync(CancellationToken cancellationToken)
    {
        var path = GetPath();
        if (!File.Exists(path))
        {
            return null;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken);
            return JsonSerializer.Deserialize<DatabaseSetupStateDocument>(json);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Atomically writes non-secret setup state.</summary>
    /// <param name="document">The state to persist.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task representing the persistence operation.</returns>
    public async Task WriteAsync(
        DatabaseSetupStateDocument document,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        var path = GetPath();
        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("The database setup state path has no directory.");
        Directory.CreateDirectory(directory);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var temporaryPath = path + ".tmp";
            await File.WriteAllTextAsync(
                temporaryPath,
                JsonSerializer.Serialize(document),
                cancellationToken);
            File.Move(temporaryPath, path, true);
        }
        finally
        {
            _gate.Release();
        }
    }

    private string GetPath()
    {
        var configured = configuration["DatabaseSetup:StateFile"];
        var path = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(environment.ContentRootPath, "App_Data", "database-setup-state.json")
            : configured;

        return Path.GetFullPath(path, environment.ContentRootPath);
    }
}

/// <summary>Contains the non-secret persisted database setup state.</summary>
/// <param name="State">The setup state.</param>
/// <param name="UpdatedAt">The UTC timestamp of the last state change.</param>
/// <param name="LastError">A safe administrator-facing failure message, when applicable.</param>
public sealed record DatabaseSetupStateDocument(
    DatabaseSetupState State,
    DateTimeOffset UpdatedAt,
    string? LastError);
