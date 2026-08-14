namespace RemoteCommerce.Infrastructure.Persistence.Services;

/// <summary>Persists non-secret database setup state in an application-owned file.</summary>
internal sealed class DatabaseSetupStateStore(
    IConfiguration configuration,
    IWebHostEnvironment environment)
{
    private readonly SemaphoreSlim _gate = new(1, 1);

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

    public async Task WriteAsync(
        DatabaseSetupStateDocument document,
        CancellationToken cancellationToken)
    {
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

internal sealed record DatabaseSetupStateDocument(
    DatabaseSetupState State,
    DateTimeOffset UpdatedAt,
    string? LastError);
